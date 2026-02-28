using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Extensions.Logging;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Core.Events;
using Wyoming.Net.Core.Server;
using Wyoming.Net.Satellite.App.Maui.Abstractions;
using Wyoming.Net.Satellite.App.Maui.ViewModels;
using Attribution = Wyoming.Net.Core.Events.Attribution;

namespace Wyoming.Net.Satellite.App.Droid;

[Service(
    Name = "Wyoming.Net.Satellite.App.Droid.SatelliteForegroundService",
    ForegroundServiceType = ForegroundService.TypeMicrophone,
    Exported = false)]
public sealed class SatelliteForegroundService : Service
{
    public const string ActionStart = "Wyoming.Net.Satellite.ACTION_START";
    private const string ActionStop = SatelliteNotificationHelper.ActionStop;

    private PowerManager.WakeLock? wakeLock;
    private AsyncTcpServer? server;

    public static SatelliteForegroundService? Instance { get; private set; }

    public WakeWordSatellite? Satellite { get; private set; }

    public bool IsActive => Satellite is not null;

    public event Action? SatelliteStateChanged;
    
    public event Action<Exception>? SatelliteErrorOccurred;
    
    public event Action? SatelliteWakeWordDetected;

    public override IBinder? OnBind(Intent? intent) => new SatelliteBinder(this);

    public override void OnCreate()
    {
        base.OnCreate();
        Instance = this;
        SatelliteNotificationHelper.CreateNotificationChannel(this);
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        if (intent?.Action == ActionStop)
        {
            _ = StopSatelliteAsync();
            return StartCommandResult.NotSticky;
        }

        var notification = SatelliteNotificationHelper.BuildNotification(this, "Starting...");

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
        {
#pragma warning disable CA1416
            StartForeground(SatelliteNotificationHelper.NotificationId, notification, ForegroundService.TypeMicrophone);
#pragma warning restore CA1416
        }
        else
        {
            StartForeground(SatelliteNotificationHelper.NotificationId, notification);
        }

        AcquireWakeLock();
        _ = StartSatelliteAsync();

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        Instance = null;
        ReleaseWakeLock();
        base.OnDestroy();
    }

    private async Task StartSatelliteAsync()
    {
        if (Satellite is not null)
        {
            return;
        }

        try
        {
            var services = IPlatformApplication.Current!.Services;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var micProvider = services.GetRequiredService<IMicInputProvider>();
            var speakerProvider = services.GetRequiredService<ISpeakerProvider>();
            var assetReader = services.GetRequiredService<IAssetReader>();
            var settingsViewModel = services.GetRequiredService<SatelliteSettingsViewModel>();

            settingsViewModel.UpdateSatelliteSettings();
            var wakeModels = await settingsViewModel.WakeSettings.GetModelsAsync(assetReader);

            Satellite = new WakeWordSatellite(wakeModels, loggerFactory, micProvider, speakerProvider);
            Satellite.StateChanged += OnSatelliteStateChanged;
            Satellite.SatelliteError += OnSatelliteError;
            Satellite.WakeWordDetected += OnWakeWordDetected;

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
                (client, srv, lf) => new SatelliteEventHandler(client, srv, lf, Satellite, info),
                loggerFactory);

            await server.StartAsync();

            SatelliteNotificationHelper.UpdateNotification(this, "Listening for wake word...");
            SatelliteStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            SatelliteErrorOccurred?.Invoke(ex);
            await StopSatelliteAsync();
        }
    }

    public async Task StopSatelliteAsync()
    {
        if (server is not null)
        {
            await server.StopAsync();
            server = null;
        }

        Satellite = null;

        SatelliteStateChanged?.Invoke();
        ReleaseWakeLock();

        StopForeground(StopForegroundFlags.Remove);
        StopSelf();
    }

    private void OnSatelliteStateChanged()
    {
        string status = Satellite switch
        {
            { IsStreaming: true } => "Streaming voice...",
            { IsPaused: true } => "Paused",
            _ when !string.IsNullOrEmpty(Satellite?.ServerId) => "Listening for wake word...",
            _ => "Waiting for server connection..."
        };

        SatelliteNotificationHelper.UpdateNotification(this, status);
        SatelliteStateChanged?.Invoke();
    }

    private async Task OnSatelliteError(Exception exception)
    {
        SatelliteErrorOccurred?.Invoke(exception);
        await StopSatelliteAsync();
    }

    private async Task OnWakeWordDetected()
    {
        SatelliteWakeWordDetected?.Invoke();

        try
        {
            var assetReader = IPlatformApplication.Current!.Services.GetRequiredService<IAssetReader>();
            var speakerProvider = IPlatformApplication.Current.Services.GetRequiredService<ISpeakerProvider>();

            var wav = await assetReader.ReadBytesAsync("ww_detected3.wav");
            var wavInfo = WavHelper.ReadWavInfo(wav);

            await speakerProvider.StartAsync(wavInfo.SampleRate, wavInfo.BytesPerSample, wavInfo.Channels);
            await speakerProvider.PlayAsync(wav, null);
            await speakerProvider.StopAsync();
        }
        catch
        {
            // Non-critical: confirmation sound failure shouldn't crash the service
        }
    }

    private void AcquireWakeLock()
    {
        if (wakeLock is not null)
        {
            return;
        }

        var powerManager = (PowerManager)GetSystemService(PowerService)!;
        wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "Wyoming.Net::SatelliteLock");
        wakeLock?.Acquire();
    }

    private void ReleaseWakeLock()
    {
        if (wakeLock is null)
        {
            return;
        }

        if (wakeLock.IsHeld)
        {
            wakeLock.Release();
        }

        wakeLock = null;
    }
}

public sealed class SatelliteBinder : Binder
{
    public SatelliteBinder(SatelliteForegroundService service)
    {
        Service = service;
    }

    public SatelliteForegroundService Service { get; }
}
