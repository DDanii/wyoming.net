namespace Wyoming.Net.Core.Audio;

public interface IMicInputProvider : IDisposable
{
    ValueTask StartRecordingAsync();

    ValueTask StopRecordingAsync();

    Task<long?> ReadAsync(byte[] buffer, CancellationToken cancellationToken);
}