using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tizen.Multimedia;
using Wyoming.Net.Core;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Satellite.App.Tz.Platform.Interop;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class TizenSpeakerProvider : ISpeakerProvider, IDisposable
{
    private readonly TizenAudioFocusManager audioFocusManager;

    private AudioPlayback? audioPlayback;

    private IntPtr audioPlaybackHandle;

    private Channel<byte[]>? playbackChannel;

    public TizenSpeakerProvider(TizenAudioFocusManager audioFocusManager)
    {
        this.audioFocusManager = audioFocusManager;
    }

    public int? Rate { get; private set; }

    public int? Width { get; private set; }

    public int? Channels { get; private set; }

    public async Task PlayAsync(byte[] samples, long? timestamp)
    {
        Asserts.IsNotNull(audioPlayback);

        if (await playbackChannel!.Writer.WaitToWriteAsync().ConfigureAwait(false))
        {
            await playbackChannel.Writer.WriteAsync(samples).ConfigureAwait(false);
        }
    }

    public ValueTask StartAsync(int rate, int width, int channels)
    {
        if (audioPlayback != null)
        {
            if (rate == Rate && width == Width && channels == Channels)
            {
                audioFocusManager.RequestTransientFocus();
                InitializePlaybackLoop();
                audioPlayback.Resume();
                return ValueTask.CompletedTask;
            }

            audioPlayback.Pause();
            audioPlayback.Dispose();
            audioPlayback = null;
        }

        audioFocusManager.RequestTransientFocus();

        audioPlayback = new AudioPlayback(rate, ToTizenChannel(channels), ToTizenSampleType(width));
        audioPlayback.ApplyStreamPolicy(audioFocusManager.Policy);
        audioPlayback.Prepare();

        audioPlaybackHandle = (IntPtr)audioPlayback.GetType()
                                                   .GetField("_handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                                                   .GetValue(audioPlayback)!;

        Rate = rate;
        Width = width;
        Channels = channels;
        InitializePlaybackLoop();

        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync()
    {
        Asserts.IsNotNull(audioPlayback);
        Asserts.IsNotNull(playbackChannel);
        
        playbackChannel!.Writer.Complete();
        return ValueTask.CompletedTask;
    }

    private void InitializePlaybackLoop()
    {
        playbackChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true
        });

        _ = Task.Factory.StartNew(PlaybackLoop);
    }

    private async Task PlaybackLoop()
    {
        while (await playbackChannel!.Reader.WaitToReadAsync())
        {
            var samples = await playbackChannel.Reader.ReadAsync();
            WritePlayback(audioPlaybackHandle, samples);
        }
        
        Reset();
    }

    private static void WritePlayback(IntPtr audioPlaybackHandle, byte[] samples)
    {
        var written = 0;
        ref var ptr = ref MemoryMarshal.GetArrayDataReference(samples);

        do
        {
            var toWrite = samples.Length - written;

            ref var data = ref Unsafe.Add(ref ptr, written);
            var ret = NativeAudio.Write(audioPlaybackHandle, ref data, (uint)toWrite);

            ret.ThrowIfFailed("Failed to write buffer");

            written += (int)ret;
        }
        while(written < samples.Length);
    }

    private static AudioChannel ToTizenChannel(int channels)
    {
        return channels switch
        {
            1 => AudioChannel.Mono,
            2 => AudioChannel.Stereo,
            3 => AudioChannel.MultiChannel3,
            4 => AudioChannel.MultiChannel4,
            _ => throw new NotImplementedException($"{channels} is not implemented")
        };
    }

    private static AudioSampleType ToTizenSampleType(int width)
    {
        return width switch
        {
            1 => AudioSampleType.U8,
            2 => AudioSampleType.S16Le,
            3 => AudioSampleType.S24Le,
            4 => AudioSampleType.S32Le,
            _ => throw new NotImplementedException($"{width} width is not implemented")
        };
    }

    private void Reset()
    {
        Asserts.IsNotNull(audioPlayback);
        Asserts.IsNotNull(playbackChannel);

        audioPlayback!.Drain();
        audioPlayback.Flush();
        audioPlayback.Pause();

        playbackChannel = null;

        audioFocusManager.AbandonFocus();
    }

    public void Dispose()
    {
        if (audioPlayback is not null)
        {
            audioPlayback.Dispose();
            audioPlayback = null;
            audioPlaybackHandle = IntPtr.Zero;
            Width = null;
            Rate = null;
            Channels = null;
        }
    }
}