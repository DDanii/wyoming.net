namespace Wyoming.Net.Satellite.App.Maui.Abstractions;

public interface ISatelliteService
{
    bool IsRunning { get; }

    event Action? StateChanged;

    event Action<Exception>? ErrorOccurred;

    event Action? WakeWordDetected;

    bool IsStreaming { get; }

    bool ServerConnected { get; }

    bool IsPaused { get; }

    bool MicMuted { get; }

    Task StartAsync();

    Task StopAsync();
}
