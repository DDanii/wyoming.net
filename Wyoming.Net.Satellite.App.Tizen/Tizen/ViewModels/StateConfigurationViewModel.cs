using System.Collections.Generic;

namespace Wyoming.Net.Satellite.App.Tz.ViewModels;

public sealed class StateConfigurationViewModel
{
    public List<string> UnactiveApps { get; set; } = new()
    {
        "com.samsung.tv.aria-video",
        "com.samsung.tv.cobalt-yt",
        "org.tizen.netflix-app",
        "org.tizen.primevideo",
        "LnLzvqrcEY.globo",
        "OGLLvqej7u.CrunchyrollWebApp",
        "MCmYXNxgcu.DisneyPlus",
        "5b8c3eb16b.BeamCTVDev"
    };

    public int WatcherIntervalSeconds { get; set; } = 5;
}
