using Wyoming.Net.Core.WebRtc.Vad;

namespace Wyoming.Net.Satellite.App.Tz.ViewModels;

public sealed class VadSettingsViewModel
{
    public bool Enabled { get; set; } = true;

    public int Type { get; set; } = (int)VadSettings.VadType.WebRtc;

    public int WebRtcMode { get; set; } = (int)VadMode.VeryAggressive;

    public bool UseEnergyGate { get; set; } = false;

    public float EnergyGateThreshold { get; set; } = 0.0002f;

    public VadSettings ToSatelliteSettings()
    {
        return new VadSettings()
        {
            Enabled = Enabled,
            Type = (VadSettings.VadType)Type,
            WebRtcMode = (VadMode)WebRtcMode,
            UseEnergyGate = UseEnergyGate,
            EnergyGateThreshold = EnergyGateThreshold,
        };
    }
}
