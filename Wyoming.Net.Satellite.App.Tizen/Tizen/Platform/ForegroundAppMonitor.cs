using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tizen.Applications;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

public sealed class ForegroundAppMonitor : IDisposable
{
    private Timer? _timer;

    private string? _currentForegroundAppId;

    private readonly ILogger _logger;

    private bool _running;

    public event Action<string>? ForegroundAppChanged;

    public ForegroundAppMonitor(ILogger logger)
    {
        _logger = logger;
    }

    public void Start(int intervalMs = 5000)
    {
        if(_running)
        {
            return;
        }

        _timer = new Timer(OnTick, null, 0, intervalMs);
        _running = true;
    }

    public void Stop()
    {
        if(!_running)
        {
            return;
        }

        _timer?.Dispose();
        _timer = null;
        _currentForegroundAppId = null;
        _running = false;
    }

    private async void OnTick(object? state)
    {
        try
        {
            var apps = await ApplicationManager.GetAllRunningApplicationsAsync();

            foreach (var app in apps)
            {
                if (app is not ApplicationRunningContext context)
                {
                    continue;
                }

                if (context.State == ApplicationRunningContext.AppState.Foreground
                    && context.ApplicationId != Constants.UiAppId
                    && context.ApplicationId != Constants.ServiceAppId
                    && context.ApplicationId != Constants.ProfilerAppId)
                {
                    if (_currentForegroundAppId != context.ApplicationId)
                    {
                        _currentForegroundAppId = context.ApplicationId;
                        ForegroundAppChanged?.Invoke(context.ApplicationId);
                    }

                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForegroundAppMonitor tick failed");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
