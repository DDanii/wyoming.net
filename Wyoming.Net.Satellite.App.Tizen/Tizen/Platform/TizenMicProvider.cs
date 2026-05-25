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
    private readonly byte[] readBuffer = new byte[MicSettings.SamplesPerChunk * sizeof(short)];

    private readonly Task<long?> cachedReadTask = Task.FromResult<long?>(null);

    private readonly AudioCapture audioCapture;

    private readonly IntPtr audioCaptureHandle;

    private readonly AudioStreamPolicy audioStreamPolicy;

    private readonly ILogger logger;

    private AudioIOState state;

    private volatile bool focusLost;

    public TizenMicProvider(ILogger logger)
    {
        audioCapture = new AudioCapture(MicSettings.Rate, AudioChannel.Mono, AudioSampleType.S16Le);
        audioCaptureHandle = (IntPtr)audioCapture.GetType()
                                                 .GetField("_handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                                                 .GetValue(audioCapture)!;

        audioStreamPolicy = new AudioStreamPolicy(AudioStreamType.Media)
        {
            FocusReacquisitionEnabled = true,
        };
        audioStreamPolicy.FocusStateChanged += OnFocusStateChanged;
        audioCapture.ApplyStreamPolicy(audioStreamPolicy);

        audioCapture.StateChanged += OnStateChanged;
        this.logger = logger;
    }

    public Task<long?> ReadAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        if (focusLost)
        {
            logger.LogInformation("Recording focus lost, skipping AudioCapture read");
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

        NativeAudio.Read(audioCaptureHandle, ref MemoryMarshal.GetReference(readBuffer.AsSpan()), readBuffer.Length).ThrowIfFailed("Failed to read audio");

        AudioOp.Pcm16ToFloat(readBuffer, MemoryMarshal.Cast<byte, float>(buffer));

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

    private void OnFocusStateChanged(object? sender, AudioStreamPolicyFocusStateChangedEventArgs args)
    {
        logger.LogInformation("Mic focus changed: {options} -> {state}", args.FocusOptions, args.FocusState);

        if (args.FocusOptions != AudioStreamFocusOptions.Recording)
        {
            return;
        }

        if (args.FocusState == AudioStreamFocusState.Released)
        {
            focusLost = true;

            if (state == AudioIOState.Running)
            {
                audioCapture.Pause();
            }
        }
        else if (args.FocusState == AudioStreamFocusState.Acquired)
        {
            focusLost = false;
        }
    }

    private void OnStateChanged(object? sender, AudioIOStateChangedEventArgs args)
    {
        logger.LogInformation("AudioCapture state changed - Previous: {prev}, Current: {cur}", args.Previous, args.Current);
        state = args.Current;
    }

    private void PrepareCapture()
    {
        if (state == AudioIOState.Idle)
        {
            audioCapture.Prepare();
        }
        audioCapture.Resume();
    }
}