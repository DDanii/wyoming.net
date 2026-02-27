using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Threading.Channels;
using Wyoming.Net.Core;
using Wyoming.Net.Satellite.ML.Models.OpenWakeWord;

namespace Wyoming.Net.Satellite;

public readonly struct OpenWakeWordModels
{
    public readonly IEmbeddingModel EmbeddingModel;
    public readonly IMelspectrogramModel MelspectrogramModel;
    public readonly IWakeWordModel WakeWordModel;

    public OpenWakeWordModels(IEmbeddingModel embeddingModel,
        IMelspectrogramModel melspectrogramModel,
        IWakeWordModel wakeWordModel)
    {
        EmbeddingModel = embeddingModel;
        MelspectrogramModel = melspectrogramModel;
        WakeWordModel = wakeWordModel;
    }
}

public sealed class OpenWakeWordService : TaskLoopRunner, IAsyncDisposable
{
    private const int ExpectedSampleSize = 1280;
    private const int SampleWindowSize = 480;

    // Input for Embedding Model
    private readonly int melSpectogramBufferSize;

    // Input por WakeWordModel
    private readonly int embeddingBufferSize;

    private readonly IEmbeddingModel embeddingModel;
    private readonly IMelspectrogramModel melspectrogramModel;
    private readonly IWakeWordModel wakeWordModel;
    private readonly SlidingWindowPcmBuffer melBuffer;
    private readonly SlidingWindowPcmBuffer embeddingBuffer;
    private readonly SlidingWindowPcmBuffer rawAudioBuffer = new(ExpectedSampleSize + SampleWindowSize);
    private readonly int maxPatience;
    private readonly float predictionThreshold;
    private readonly IWakeWordPredictionHandler predictionHandler;

    private readonly Channel<AudioTask<float>> channel;

    public OpenWakeWordService(
        OpenWakeWordModels models,
        IWakeWordPredictionHandler predictionHandler,
        ILogger<OpenWakeWordService> logger,
        int maxPatience = 15,
        float predictionThreshold = 0.5f)
        : base(logger, TaskLoopRunnerOptions.RestartOnFail)
    {
        embeddingModel = models.EmbeddingModel;
        melspectrogramModel = models.MelspectrogramModel;
        wakeWordModel = models.WakeWordModel;
        embeddingBufferSize = models.WakeWordModel.FlatShapeSize;
        embeddingBuffer = new SlidingWindowPcmBuffer(embeddingBufferSize);
        melSpectogramBufferSize = models.EmbeddingModel.FlatShapeSize;
        melBuffer = new SlidingWindowPcmBuffer(melSpectogramBufferSize);

        this.predictionThreshold = predictionThreshold;
        this.maxPatience = maxPatience;
        this.predictionHandler = predictionHandler;

        channel = Channel.CreateBounded<AudioTask<float>>(new BoundedChannelOptions(32)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest,
            AllowSynchronousContinuations = false
        });
    }

    public void AppendPcm(ReadOnlySpan<float> samples)
    {
        if (samples.Length != ExpectedSampleSize)
        {
            throw new ArgumentException($"Samples must be of size {ExpectedSampleSize}");
        }

        rawAudioBuffer.Append(samples, SampleWindowSize);
        channel.Writer.TryWrite(new AudioTask<float>(rawAudioBuffer.Span));
    }

    protected override async Task LoopAsync()
    {
        int patience = maxPatience;

        while (!CancellationTokenSource!.IsCancellationRequested)
        {
            if (!await channel.Reader.WaitToReadAsync(CancellationTokenSource!.Token))
            {
                continue;
            }

            using var chunk = await channel.Reader.ReadAsync(CancellationTokenSource!.Token);
            float prediction = Predict(chunk.Buffer.Span);

            logger.LogDebug("Prediction: {prediction}", prediction);

            if (patience > 0)
            {
                patience--;
                continue;
            }

            if (patience == 0 && prediction >= predictionThreshold && !CancellationTokenSource.IsCancellationRequested)
            {
                patience = maxPatience;
                await predictionHandler.OnPredictionAsync();
            }
        }
    }

    int silenceFrames = 0;

    private float Predict(ReadOnlySpan<float> samples)
    {
        if (IsSilence(samples))
        {
            silenceFrames = Math.Max(silenceFrames, 0);

            if(++silenceFrames == 5)
            {
                melBuffer.Clear();
                embeddingBuffer.Clear();
            }
            return 0f;
        }

        silenceFrames = 0;

        // samples -> MelspectrogramModel -> EmbeddingModel -> WakeWordModel

        Span<float> melOutputBuffer = stackalloc float[melspectrogramModel.FlattenedOutputSize];
        melspectrogramModel.GenerateSpectrogram(samples, melOutputBuffer);

        melBuffer.Append(melOutputBuffer, melSpectogramBufferSize - melOutputBuffer.Length);

        Span<float> embeddingOutputBuffer = stackalloc float[embeddingModel.FlattenedOutputSize];
        embeddingModel.GenerateAudioEmbeddings(melBuffer.Span, embeddingOutputBuffer);
        embeddingBuffer.Append(embeddingOutputBuffer, embeddingBufferSize - embeddingOutputBuffer.Length);

        float prediction = wakeWordModel.Predict(embeddingBuffer.Span);

        return prediction;
    }

    private bool IsSilence(ReadOnlySpan<float> samples)
    {
        float energy = 0f;
        int zeroCrossings = 0;

        for (int i = 1; i < samples.Length; i++)
        {
            energy += samples[i] * samples[i];
            if ((samples[i] > 0) != (samples[i - 1] > 0))
                zeroCrossings++;
        }

        energy /= samples.Length;
        float zcr = (float)zeroCrossings / samples.Length;

        // Low energy = silence; high energy + very high ZCR = noise, not speech
        if (energy < 0.0008)
            return true;
        if (zcr > 0.4f)
            return true; // likely noise, not speech

        return false;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopAsync();

            embeddingModel.Dispose();
            melspectrogramModel.Dispose();
            wakeWordModel.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error disposing openwakeword service");
        }
    }
}

sealed class SlidingWindowPcmBuffer
{
    private readonly float[] buffer;

    public SlidingWindowPcmBuffer(int maxSize)
    {
        buffer = new float[maxSize];
    }

    public void Append(ReadOnlySpan<float> newData, int windowSize)
    {
        var span = buffer.AsSpan();
        span.Slice(buffer.Length - windowSize).CopyTo(span); // Move old data to start

        newData.CopyTo(span.Slice(windowSize));  // Put new data at the end
    }

    public ReadOnlySpan<float> Span => buffer.AsSpan();

    public void Clear()
    {
        buffer.AsSpan().Clear();
    }
}

sealed class AudioTask<T> : IDisposable
    where T : struct
{
    private readonly int size;
    private readonly T[] chunk;

    public AudioTask(ReadOnlySpan<T> chunk)
    {
        size = chunk.Length;
        this.chunk = ArrayPool<T>.Shared.Rent(size);
        chunk.CopyTo(this.chunk);
    }

    public Memory<T> Buffer => new(chunk, 0, size);

    private void Dispose(bool disposing)
    {
        ArrayPool<T>.Shared.Return(chunk);

        if (disposing)
        {
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}