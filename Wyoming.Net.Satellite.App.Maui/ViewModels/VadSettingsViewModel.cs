using CommunityToolkit.Mvvm.ComponentModel;
using Wyoming.Net.Core.WebRtc.Vad;
using static Wyoming.Net.Satellite.VadSettings;

namespace Wyoming.Net.Satellite.App.Maui.ViewModels;

public partial class VadSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    bool enabled;

    [ObservableProperty]
    string type = nameof(VadType.WebRtc);

    [ObservableProperty]
    string webRtcMode = nameof(VadMode.VeryAggressive);

    [ObservableProperty]
    bool useEnergyGate;

    [ObservableProperty]
    float energyGateThreshold = 0.0008f;

    [ObservableProperty]
    float energyGateZcr = 0.4f;

    public VadSettings ToSatelliteSettings()
    {
        return new VadSettings
        {
            Enabled = Enabled,
            Type = Enum.Parse<VadType>(Type),
            WebRtcMode = Enum.Parse<VadMode>(WebRtcMode),
            UseEnergyGate = UseEnergyGate,
            EnergyGateThreshold = EnergyGateThreshold,
            EnergyGateZcr = EnergyGateZcr,
        };
    }
}
