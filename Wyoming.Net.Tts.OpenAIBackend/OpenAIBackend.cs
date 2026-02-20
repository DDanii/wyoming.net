using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using OpenAI;
using OpenAI.Audio;
using Wyoming.Net.Core;
using Wyoming.Net.Core.Audio;

public sealed class OpenAIBackend : ITextToSpeechProvider
{
    private readonly string apiKey;

    public OpenAIBackend(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public int SampleRate => 24000;

    public int ChannelCount => 1;

    public int Width => sizeof(short);

    private AudioClient? client;
    
    [Experimental("OPENAI001")]
    public async Task SynthesizeAsync(string text, OnStreamAsync callback)
    {
        Asserts.IsNotNull(client, "Expected client to be initialized");
        
        var result = await client!.GenerateSpeechAsync(text, GeneratedSpeechVoice.Nova, new SpeechGenerationOptions()
        {
            ResponseFormat = GeneratedSpeechFormat.Pcm,
            Instructions = "Esse é apenas um chunk de uma frase, responda como tal tendo em mente as pausas corretas de acordo com o final desse chunk"
        });
        
        await callback(result.Value.ToMemory());
    }

    public Task InitializeAsync(string model, string voice)
    {
        client = new AudioClient("gpt-4o-mini-tts", new ApiKeyCredential(apiKey!));
        return Task.CompletedTask;
    }
}