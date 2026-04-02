namespace Wyoming.Net.Satellite.App.Tz.ViewModels;

public sealed class ControlPanelViewModel
{
    public string RemoteLogIp { get; set; } = "192.168.1.148";

    public int RemoteLogPort { get; set; } = 5005;

    public bool DebugAudioEnabled { get; set; } = false;
}
