using Microsoft.Extensions.Logging;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Core.Events;
using Wyoming.Net.Core.Server;
using Wyoming.Net.Satellite.App.Maui.Abstractions;
using Wyoming.Net.Satellite.App.Maui.ViewModels;

namespace Wyoming.Net.Satellite.App.Maui;

public sealed class InProcessSatelliteService : ISatelliteService
{
    private readonly ILoggerFactory loggerFactory;
    private readonly IMicInputProvider micProvider;
    private readonly ISpeakerProvider speakerProvider;
    private readonly IAssetReader assetReader;
    private readonly SatelliteSettingsViewModel settingsViewModel;

    private WakeWordSatellite? satellite;
    private AsyncTcpServer? server;

    public InProcessSatelliteService(
        ILoggerFactory loggerFactory,
        IMicInputProvider micProvider,
        ISpeakerProvider speakerProvider,
        IAssetReader assetReader,
        SatelliteSettingsViewModel settingsViewModel)
    {
        this.loggerFactory = loggerFactory;
        this.micProvider = micProvider;
        this.speakerProvider = speakerProvider;
        this.assetReader = assetReader;
        this.settingsViewModel = settingsViewModel;
    }

    public bool IsRunning => satellite?.IsRunning == true;
    
    public bool IsStreaming => satellite?.IsStreaming == true;
    
    public bool ServerConnected => !string.IsNullOrEmpty(satellite?.ServerId);
    
    public bool IsPaused => satellite?.IsPaused ?? true;
    
    public bool MicMuted => satellite?.MicMuted ?? false;

    public event Action? StateChanged;
    
    public event Action<Exception>? ErrorOccurred;
    
    public event Action? WakeWordDetected;

    public async Task StartAsync()
    {
        if (satellite is not null)
            return;

        settingsViewModel.UpdateSatelliteSettings();
        var wakeModels = await settingsViewModel.WakeSettings.GetModelsAsync(assetReader);

        satellite = new WakeWordSatellite(wakeModels, loggerFactory, micProvider, speakerProvider);
        satellite.StateChanged += OnStateChanged;
        satellite.SatelliteError += OnError;
        satellite.WakeWordDetected += OnWakeWordDetected;

        var info = new Info(new Core.Events.Satellite()
        {
            ActiveWakeWords = [SatelliteSettings.Wake.Name!],
            Attribution = new Attribution
            {
                Name = "Guilherme Pohlmann da Rosa",
                Url = "https://github.com/guilherme-pohlmann/wyoming-net"
            },
            Description = settingsViewModel.Description,
            Name = settingsViewModel.Name!,
            HasVad = false,
            Installed = true,
            MaxActiveWakeWords = 1,
            SupportsTrigger = true,
            Version = "0.0.1",
            Area = settingsViewModel.Area,
        });

        server = new AsyncTcpServer(
            "0.0.0.0",
            settingsViewModel.Port,
            (client, srv, lf) => new SatelliteEventHandler(client, srv, lf, satellite, info),
            loggerFactory);

        await server.StartAsync();
        StateChanged?.Invoke();
    }

    public async Task StopAsync()
    {
        if (server is not null)
        {
            await server.StopAsync();
            server = null;
        }

        satellite = null;
        StateChanged?.Invoke();
    }

    private void OnStateChanged() => StateChanged?.Invoke();

    private async Task OnError(Exception ex)
    {
        await StopAsync();
        ErrorOccurred?.Invoke(ex);
    }

    private async Task OnWakeWordDetected()
    {
        WakeWordDetected?.Invoke();

        try
        {
            var wav = await assetReader.ReadBytesAsync("ww_detected3.wav");
            var wavInfo = WavHelper.ReadWavInfo(wav);

            await speakerProvider.StartAsync(wavInfo.SampleRate, wavInfo.BytesPerSample, wavInfo.Channels);
            await speakerProvider.PlayAsync(wav, null);
            await speakerProvider.StopAsync();
        }
        catch
        {
            // Non-critical
        }
    }
}
