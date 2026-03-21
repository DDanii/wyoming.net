using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Wyoming.Net.Core;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Core.WebRtc;
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
    private const int Fps = 12;

    private const int ExpectedSampleSize = MicSettings.SamplesPerChunk;
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
    private readonly IWakeWordPredictionHandler predictionHandler;
    private readonly WebRtcVad? webRtcVad;

    private readonly Channel<AudioTask<float>> channel;

    private int silenceFrames = 0;
    private int warmupFrames = 0;

    public OpenWakeWordService(
        OpenWakeWordModels models,
        IWakeWordPredictionHandler predictionHandler,
        ILogger<OpenWakeWordService> logger)
        : base(logger, TaskLoopRunnerOptions.RestartOnFail | TaskLoopRunnerOptions.LongRunning)
    {
        embeddingModel = models.EmbeddingModel;
        melspectrogramModel = models.MelspectrogramModel;
        wakeWordModel = models.WakeWordModel;
        embeddingBufferSize = models.WakeWordModel.FlatShapeSize;
        embeddingBuffer = new SlidingWindowPcmBuffer(embeddingBufferSize);
        melSpectogramBufferSize = models.EmbeddingModel.FlatShapeSize;
        melBuffer = new SlidingWindowPcmBuffer(melSpectogramBufferSize);

        this.predictionHandler = predictionHandler;

        channel = Channel.CreateBounded<AudioTask<float>>(new BoundedChannelOptions(32)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.DropOldest,
            AllowSynchronousContinuations = false
        });

        if (SatelliteSettings.Vad.Enabled && SatelliteSettings.Vad.Type.HasFlag(VadSettings.VadType.WebRtc))
        {
            webRtcVad = new WebRtcVad()
            {
                SampleRate = MicSettings.Rate,
                Mode = SatelliteSettings.Vad.WebRtcMode
            };
        }
    }

    public void AppendPcm(ReadOnlySpan<float> samples)
    {
        if (samples.Length != ExpectedSampleSize)
        {
            throw new ArgumentException($"Samples must be of size {ExpectedSampleSize}");
        }

        bool energySilence = SatelliteSettings.Vad.UseEnergyGate && IsSilence(samples);
        bool vadSilence = webRtcVad is not null && !VadIsSpeech(samples);
        bool isSilent = energySilence || vadSilence;

        rawAudioBuffer.Append(samples, SampleWindowSize);

        if (isSilent)
        {
            if (warmupFrames > 0)
            {
                warmupFrames--;
            }
            else
            {
                if (++silenceFrames == Fps * 5) // 5 seconds
                {
                    warmupFrames = 0;
                    melBuffer.Clear();
                    embeddingBuffer.Clear();
                }

                return;
            }

        }
        else if (warmupFrames == 0)
        {
            warmupFrames = Fps * 5;
        }

        silenceFrames = 0;
        channel.Writer.TryWrite(new AudioTask<float>(rawAudioBuffer.Span));
    }

    protected override async Task LoopAsync()
    {
        int speechFrames = 0;
        int patienceRemaining = 0;
        float predictionThreshold = SatelliteSettings.Wake.PredictionThreshold;

        while (!CancellationTokenSource!.IsCancellationRequested)
        {
            if (!await channel.Reader.WaitToReadAsync(CancellationTokenSource!.Token))
            {
                continue;
            }

            using var chunk = await channel.Reader.ReadAsync(CancellationTokenSource!.Token);
            float prediction = Predict(chunk.Buffer.Span);

            logger.LogDebug("Prediction: {prediction}", prediction);

            if (prediction >= predictionThreshold && !CancellationTokenSource.IsCancellationRequested)
            {
                if (patienceRemaining > 0)
                {
                    logger.LogInformation("Skipping prediction, patience: {p}", patienceRemaining);
                    continue;
                }

                speechFrames++;

                if (speechFrames < SatelliteSettings.Wake.MinSpeechFrames)
                {
                    continue;
                }

                await predictionHandler.OnPredictionAsync();

                speechFrames = 0;
                patienceRemaining = SatelliteSettings.Wake.Patience;
            }
            else
            {
                speechFrames = Math.Max(--speechFrames, 0);
                patienceRemaining = Math.Max(--patienceRemaining, 0);
            }
        }
    }

    protected override ValueTask OnStopAsync()
    {
        warmupFrames = 0;
        silenceFrames = 0;
        rawAudioBuffer.Clear();
        melBuffer.Clear();
        embeddingBuffer.Clear();

        return ValueTask.CompletedTask;
    }

    private float Predict(ReadOnlySpan<float> samples)
    {
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

    private static bool IsSilence(ReadOnlySpan<float> samples)
    {
        float energy = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            energy += samples[i] * samples[i];
        }

        energy /= samples.Length;

        return energy < SatelliteSettings.Vad.EnergyGateThreshold;
    }

    private bool VadIsSpeech(ReadOnlySpan<float> samples)
    {
        // We want to find speech on the entire 80ms audio frame
        // not on an individual 30ms chunk

        Span<byte> frames = stackalloc byte[samples.Length * 2];
        AudioOp.FloatToPcm16(samples, frames);

        ReadOnlySpan<short> pcm = MemoryMarshal.Cast<byte, short>(frames);

        const int chunkSize = 480; // 30ms at 16kHz

        for (int i = 0; i + chunkSize <= pcm.Length; i += chunkSize)
        {
            if (!webRtcVad!.Process(pcm.Slice(i, chunkSize)))
            {
                return false;
            }
        }

        int remaining = pcm.Length % chunkSize; // 1760 % 480 = 320 (20ms)
        return webRtcVad!.Process(pcm.Slice(pcm.Length - remaining, remaining));
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

    public ReadOnlyMemory<T> Buffer => new(chunk, 0, size);

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(chunk);
    }
}