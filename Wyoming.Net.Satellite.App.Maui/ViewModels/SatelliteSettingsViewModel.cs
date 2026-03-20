using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using Wyoming.Net.Core.WebRtc.Vad;

namespace Wyoming.Net.Satellite.App.Maui.ViewModels;

public partial class SatelliteSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    WakeSettingsViewModel wakeSettings = new();

    [ObservableProperty]
    VadSettingsViewModel vadSettings = new();

    [ObservableProperty]
    string? area;

    [ObservableProperty]
    string? name;

    [ObservableProperty]
    string? description;

    [ObservableProperty]
    int port = 10568;

    public bool IsValid(out string? message)
    {
        message = null;

        if (string.IsNullOrEmpty(Area))
        {
            message = "Please enter Area";
            return false;
        }

        if (string.IsNullOrEmpty(Name))
        {
            message = "Please enter Name";
            return false;
        }

        if(Port < 0 || Port > 65535)
        {
            message = "Port number is invalid";
            return false;
        }

        return WakeSettings.IsValid(out message);
    }

    public void Save()
    {  
        File.WriteAllText(GetSettingsFilePath(), JsonSerializer.Serialize(this));
    }

    public void UpdateSatelliteSettings()
    {
        SatelliteSettings.Wake.Name = WakeSettings.Model;
        SatelliteSettings.Wake.MinSpeechFrames = WakeSettings.MinSpeechFrames;
        SatelliteSettings.Wake.Patience = WakeSettings.Patience;
        SatelliteSettings.Wake.PredictionThreshold = WakeSettings.PredictionThreshold;
        SatelliteSettings.Wake.RefractorySeconds = WakeSettings.RefractorySeconds;

        SatelliteSettings.Vad.Enabled = VadSettings.Enabled;
        SatelliteSettings.Vad.Type = VadSettings.Type switch
        {
            _ => Satellite.VadSettings.VadType.WebRtc
        };
        SatelliteSettings.Vad.WebRtcMode = VadSettings.WebRtcMode switch
        {
            "Quality" => VadMode.Quality,
            "LowBitrate" => VadMode.LowBitrate,
            "Aggressive" => VadMode.Aggressive,
            _ => VadMode.LowBitrate
        };
        SatelliteSettings.Vad.UseEnergyGate = VadSettings.UseEnergyGate;
        SatelliteSettings.Vad.EnergyGateThreshold = VadSettings.EnergyGateThreshold;
    }

    public static SatelliteSettingsViewModel Load()
    {
        var file = GetSettingsFilePath();

        try
        {
            if (File.Exists(file))
            {
                return JsonSerializer.Deserialize<SatelliteSettingsViewModel>(File.ReadAllText(file)) ?? new();
            }

            return new();
        }
        catch 
        {
            return new();
        }
    }

    private static string GetSettingsFilePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "settings.json");
    }
}

