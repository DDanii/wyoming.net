using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wyoming.Net.Core.Events;
using Wyoming.Net.Core.Server;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal static class TizenServer
{
    public static AsyncTcpServer? Singleton;

    public static bool CreateSingleton(WakeWordSatellite satellite, SatelliteSettingsViewModel settingsViewModel, ILoggerFactory loggerFactory)
    {
        var settings = settingsViewModel.ToSatelliteSettings();

        var info = new Info(new Core.Events.Satellite()
        {
            ActiveWakeWords = new string[] { settings.Wake.Name! },
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

        Singleton = new AsyncTcpServer(
           "0.0.0.0",
           settingsViewModel.Port,
           (client, server, loggerFactory) => new SatelliteEventHandler(client, server, loggerFactory, satellite, info),
           loggerFactory);

        return true;
    }
}
