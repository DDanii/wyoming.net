namespace Wyoming.Net.Core.Audio;

public delegate Task OnStreamAsync(ReadOnlyMemory<byte> samples);

public interface ITextToSpeechProvider : IAsyncDisposable
{
    int SampleRate { get; }
    
    int ChannelCount { get; }
    
    int Width { get; }
    
    Task SynthesizeAsync(string text, OnStreamAsync callback);

    Task InitializeAsync(string model, string voice);
}