using System.Collections.Generic;

namespace Wyoming.Net.Satellite.App.Tz.ViewModels;

public sealed class StateConfigurationViewModel
{
    public List<string> UnactiveApps { get; set; } = new();

    public int WatcherIntervalSeconds { get; set; } = 5;
}
