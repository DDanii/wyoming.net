using Android.Content;
using Android.OS;
using Wyoming.Net.Satellite.App.Maui.Abstractions;
using Wyoming.Net.Satellite.App.Maui.ViewModels;

namespace Wyoming.Net.Satellite.App.Droid;

public sealed class DroidSatelliteService : Java.Lang.Object, ISatelliteService, IServiceConnection
{
    private readonly SatelliteSettingsViewModel settingsViewModel;
    private SatelliteForegroundService? boundService;

    public DroidSatelliteService(SatelliteSettingsViewModel settingsViewModel)
    {
        this.settingsViewModel = settingsViewModel;
    }

    public bool IsRunning => boundService is { IsActive: true };
    
    public bool IsStreaming => boundService?.Satellite?.IsStreaming == true;
    
    public bool ServerConnected => !string.IsNullOrEmpty(boundService?.Satellite?.ServerId);
    
    public bool IsPaused => boundService?.Satellite?.IsPaused ?? true;
    
    public bool MicMuted => boundService?.Satellite?.MicMuted ?? false;

    public event Action? StateChanged;
    
    public event Action<Exception>? ErrorOccurred;
    
    public event Action? WakeWordDetected;

    public Task StartAsync()
    {
        if (!settingsViewModel.IsValid(out _))
        {
            return Task.CompletedTask;
        }

        var context = Android.App.Application.Context;

        var intent = new Intent(context, typeof(SatelliteForegroundService));
        intent.SetAction(SatelliteForegroundService.ActionStart);

        context.StartForegroundService(intent);
        context.BindService(intent, this, Bind.AutoCreate);

        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (boundService is not null)
        {
            await boundService.StopSatelliteAsync();
        }

        try
        {
            Android.App.Application.Context.UnbindService(this);
        }
        catch (Java.Lang.IllegalArgumentException)
        {
            // Already unbound
        }

        boundService = null;
        StateChanged?.Invoke();
    }

    void IServiceConnection.OnServiceConnected(ComponentName? name, IBinder? service)
    {
        if (service is not SatelliteBinder binder)
        {
             return;
        }
        
        boundService = binder.Service;
        boundService.SatelliteStateChanged += OnStateChanged;
        boundService.SatelliteErrorOccurred += OnErrorOccurred;
        boundService.SatelliteWakeWordDetected += OnWakeWordDetected;
        StateChanged?.Invoke();
    }

    void IServiceConnection.OnServiceDisconnected(ComponentName? name)
    {
        if (boundService is not null)
        {
            boundService.SatelliteStateChanged -= OnStateChanged;
            boundService.SatelliteErrorOccurred -= OnErrorOccurred;
            boundService.SatelliteWakeWordDetected -= OnWakeWordDetected;
        }

        boundService = null;
        StateChanged?.Invoke();
    }

    private void OnStateChanged() => StateChanged?.Invoke();
    
    private void OnErrorOccurred(Exception ex) => ErrorOccurred?.Invoke(ex);
    
    private void OnWakeWordDetected() => WakeWordDetected?.Invoke();
}
