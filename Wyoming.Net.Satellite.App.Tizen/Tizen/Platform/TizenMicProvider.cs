using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.Multimedia;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Satellite.App.Tz.Platform.Interop;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class TizenMicProvider : IMicInputProvider
{
    private readonly Task<long?> cachedReadTask = Task.FromResult<long?>(null);

    private readonly AudioCapture audioCapture;

    private readonly IntPtr audioCaptureHandle;

    private readonly AudioStreamPolicy audioStreamPolicy;

    private readonly ILogger logger;

    private AudioIOState state;

    public TizenMicProvider(ILogger logger)
    {
        audioCapture = new AudioCapture(Rate, AudioChannel.Mono, AudioSampleType.S16Le);
        audioCaptureHandle = (IntPtr)audioCapture.GetType()
                                                 .GetField("_handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                                                 .GetValue(audioCapture)!;


        audioStreamPolicy = new AudioStreamPolicy(AudioStreamType.VoiceRecognition)
        {
            FocusReacquisitionEnabled = true,
        };
        audioCapture.ApplyStreamPolicy(audioStreamPolicy);

        audioCapture.StateChanged += OnStateChanged;
        this.logger = logger;
    }

    public int Rate => 16000;

    public int Channels => 1;

    public int Width => sizeof(float);

    public Task<long?> ReadAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        // We are on playback
        if (audioStreamPolicy.PlaybackFocusState == AudioStreamFocusState.Acquired)
        {
            logger.LogInformation("Playback has focus, skipping AudioCapture read");
            return cachedReadTask;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return cachedReadTask;
        }

        switch (state)
        {
            case AudioIOState.Idle:
                logger.LogInformation("Read called in Idle state, preparing audio capture");
                PrepareCapture();
                break;

            case AudioIOState.Paused:
                logger.LogInformation("Read called in Paused state, resuming audio capture");
                audioCapture.Resume();
                break;
        }


        var sampleCount = buffer.Length / Width;
        Span<byte> audio = stackalloc byte[sampleCount * sizeof(short)];

        NativeAudio.Read(audioCaptureHandle, ref MemoryMarshal.GetReference(audio), audio.Length).ThrowIfFailed("Failed to read audio");

        AudioOp.Pcm16ToFloat(audio, MemoryMarshal.Cast<byte, float>(buffer));
        return cachedReadTask;
    }

    public ValueTask StartRecordingAsync()
    {
        PrepareCapture();
        return ValueTask.CompletedTask;
    }

    public ValueTask StopRecordingAsync()
    {
        audioCapture.Pause();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        audioCapture.Dispose();
    }

    private void OnStateChanged(object? sender, AudioIOStateChangedEventArgs args)
    {
        logger.LogInformation("AudioCapture state changed - Previous: {prev}, Current: {cur}", args.Previous, args.Current);
        state = args.Current;
    }

    private void PrepareCapture()
    {
        audioCapture.Prepare();
        audioCapture.Resume();
    }
}