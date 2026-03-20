using Tizen.Applications;
using Wyoming.Net.Satellite.App.Tz.Platform;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz;

public sealed class ProfilerApp : ServiceApplication
{
    private TizenProfiler? _profiler;

    protected override void OnCreate()
    {
        base.OnCreate();

        var settings = SatelliteSettingsViewModel.Load();
        RemoteLogger.InitSingleton(
            settings.ControlPanel.RemoteLogIp,
            settings.ControlPanel.RemoteLogPort);

        _profiler = new TizenProfiler(Constants.ServiceAppId, 1000);
    }

    protected override void OnTerminate()
    {
        _profiler?.Dispose();
        base.OnTerminate();
    }
}
