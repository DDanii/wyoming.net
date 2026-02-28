using System.Threading.Channels;
using Android.Media;
using Wyoming.Net.Core;
using Wyoming.Net.Core.Audio;
using Encoding =  Android.Media.Encoding;

namespace Wyoming.Net.Satellite;

public sealed class DroidSpeakerProvider : ISpeakerProvider
{
    private readonly AudioFocusManager audioFocusManager = new();
    private AudioTrack? track;
    private Channel<byte[]>? playbackChannel;
    
    public int? Rate { get; private set; }

    public int? Width { get; private set; }

    public int? Channels { get; private set; }

    public async Task PlayAsync(byte[] samples, long? timestamp)
    {
        Asserts.IsNotNull(track, "StartAsync should have been called at this point");
        Asserts.IsNotNull(playbackChannel, "StartAsync should have been called at this point");

        if (await playbackChannel!.Writer.WaitToWriteAsync().ConfigureAwait(false))
        {
            await playbackChannel.Writer.WriteAsync(samples).ConfigureAwait(false);
        }
    }

    public ValueTask StartAsync(int sampleRate, int width, int channels)
    {
        if(track is not null)
        {
            return ValueTask.CompletedTask;
        }

        audioFocusManager.RequestTransientFocus();

        ChannelOut channelOut = channels == 1 ? ChannelOut.Mono : ChannelOut.Stereo;
        Encoding encoding = GetEncoding(width);

        var audioFormat = new AudioFormat.Builder()
            .SetSampleRate(sampleRate)!
            .SetEncoding(encoding)!
            .SetChannelMask(channelOut)
            .Build();

        var audioAttributesBuilder = new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Media)!
            .SetContentType(AudioContentType.Speech);

        int minBuffer = AudioTrack.GetMinBufferSize(sampleRate, channelOut, encoding);

        track = new AudioTrack.Builder()
            .SetAudioAttributes(audioAttributesBuilder!.Build()!)
            .SetAudioFormat(audioFormat!)
            .SetBufferSizeInBytes(minBuffer)
            .SetTransferMode(AudioTrackMode.Stream)
            .Build();

        track.Play();

        Rate = sampleRate;
        Width = width;
        Channels = channels;
        
        playbackChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true
        });

        _ = Task.Factory.StartNew(PlaybackLoop);
        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync()
    {
        Asserts.IsNotNull(playbackChannel, "StartAsync should have been called at this point");
        playbackChannel!.Writer.Complete();
        
        return ValueTask.CompletedTask;
    }

    private void Reset()
    {
        track?.Stop();
        track?.Dispose();
        track = null;

        Rate = null;
        Width = null;
        Channels = null;
        
        playbackChannel = null;

        audioFocusManager.AbandonFocus();
    }

    private async Task PlaybackLoop()
    {
        while (await playbackChannel!.Reader.WaitToReadAsync())
        {
            var samples = await playbackChannel.Reader.ReadAsync();
            await track!.WriteAsync(samples, 0, samples.Length, WriteMode.Blocking).ConfigureAwait(false);
        }
        
        Reset();
    }
    
    private static Encoding GetEncoding(int width)
    {
        return width switch
        {
            1 => Encoding.Pcm8bit,
            2 => Encoding.Pcm16bit,
            4 => Encoding.PcmFloat,
            _ => throw new ArgumentException($"Unsupported width: {width}")
        };
    }
}
