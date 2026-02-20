using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Wyoming.Net.Core;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Core.Events;
using Wyoming.Net.Core.Server;

namespace Wyoming.Net.Tts;

public sealed class SynthesizeEventHandler : AsyncEventHandler
{
   private static readonly Event SynthesizeStoppedEvent = new SynthesizeStopped().ToEvent();
   private static readonly Event AudioStopEvent = new AudioStop().ToEvent();
    
    private ITextToSpeechProvider? inferenceBackend;
    private readonly Info wyomingInfo;
    private readonly WyomingStreamWriter writer;
    private readonly Func<ITextToSpeechProvider> backendFactory;

    private Channel<string>? channel;

    private bool isStreaming;
    
    public SynthesizeEventHandler(
        TcpClient client, 
        AsyncTcpServer server,
        ILoggerFactory loggerFactory,
        Func<ITextToSpeechProvider> backendFactory,
        Info wyomingInfo) 
        : base(client, server, loggerFactory)
    {
        this.wyomingInfo = wyomingInfo;
        this.backendFactory = backendFactory;
        writer = new WyomingStreamWriter(client.GetStream());
    }

    protected override async Task<bool> HandleEventAsync(Event ev, CancellationToken cancellationToken)
    {
        if (Describe.IsType(ev.Type))
        {
            await writer.WriteEventAsync(wyomingInfo.ToEvent());
            return true;
        }
        
        if (Synthesize.IsType(ev.Type))
        {
            if (isStreaming)
            {
                return true;
            }
            
            var synthesize = Synthesize.FromEvent(ev);
            
            await StartAudioAsync(synthesize.Voice?.Name);
            await HandleSynthesizeAsync(synthesize.Text, cancellationToken);

            return true;
        }
        
        if (SynthesizeStart.IsType(ev.Type))
        {
            var synthesizeStart = SynthesizeStart.FromEvent(ev);
            await StartAudioAsync(synthesizeStart.Voice?.Name);
            isStreaming = true;
            channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = true,
            });
            
            _ = Task.Run(SynthesizeLoopAsync, cancellationToken);
            return true;
        }

        if (SynthesizeChunk.IsType(ev.Type))
        {
            Asserts.IsNotNull(inferenceBackend, "Expected inference backend to not be null at this point");
            var synthesizeChunk = SynthesizeChunk.FromEvent(ev);
            
            await channel!.Writer.WriteAsync(synthesizeChunk.Text!, cancellationToken);
            return true;
        }

        if (SynthesizeStop.IsType(ev.Type))
        {
            Asserts.IsNotNull(channel, "Channel should not be null at this point");
            
            isStreaming = false;
            channel!.Writer.Complete();
        }

        return true;
    }

    private async Task SynthesizeLoopAsync()
    {
        Asserts.IsNotNull(channel, "Channel should not be null at this point");
        
        while (await channel!.Reader.WaitToReadAsync())
        {
            var chunk = await channel.Reader.ReadAsync();
            
            logger.LogDebug("Synthesizing {text}", chunk);
            
            await HandleSynthesizeAsync(chunk, CancellationToken.None);
        }
        
        await StopAudioAsync();
        await writer.WriteEventAsync(SynthesizeStoppedEvent);
    }
    
    private async Task HandleSynthesizeAsync(string? text, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(text) || cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            await inferenceBackend!.SynthesizeAsync(text, OnSpeechAsync);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to synthesize chunk: {chunk}", text);
        }
    }

    private async Task OnSpeechAsync(ReadOnlyMemory<byte> samples)
    {
        AudioChunk audioChunk = AudioChunk.FromPcm16(samples.Span, null, inferenceBackend!.SampleRate, inferenceBackend.ChannelCount);
        await writer.WriteEventAsync(audioChunk.ToEvent());
    }

    protected override ValueTask OnDisconnectAsync()
    {
        return StopAudioAsync();
    }

    private async Task StartAudioAsync(string? voice)
    {
        Asserts.IsNull(inferenceBackend, "Expected inference backend to be null at this point");
        
        voice = string.IsNullOrEmpty(voice) ? UserSettings.DefaultVoice : voice;
        inferenceBackend = backendFactory();
        await inferenceBackend.InitializeAsync(UserSettings.Model, voice!);
        
        await writer.WriteEventAsync(new AudioStart()
        {
            Rate = inferenceBackend!.SampleRate,
            Channels = inferenceBackend.ChannelCount,
            Timestamp = null,
            Width = inferenceBackend.Width,
        }.ToEvent());
    }

    private async ValueTask StopAudioAsync()
    {
        if (inferenceBackend is not null)
        {
            await inferenceBackend!.DisposeAsync();
            inferenceBackend = null;
        }

        if (isStreaming)
        {
            await writer.WriteEventAsync(AudioStopEvent);
            isStreaming = false;
        }
    }
}