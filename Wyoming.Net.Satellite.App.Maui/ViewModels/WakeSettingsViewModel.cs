using CommunityToolkit.Mvvm.ComponentModel;
using Wyoming.Net.Core;
using Wyoming.Net.Satellite.App.Maui.Abstractions;
using Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Onnx;

namespace Wyoming.Net.Satellite.App.Maui.ViewModels;

public partial class WakeSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    string? model;

    [ObservableProperty]
    int refractorySeconds = 5;

    [ObservableProperty]
    int maxPatience = 20;

    [ObservableProperty]
    float predictionThreshold = 0.5f;

    public bool IsValid(out string? message)
    {
        message = null;

        if(string.IsNullOrEmpty(Model))
        {
            message = "Please enter wake word model";
            return false;  
        }

        return true;
    }

    public async Task<OpenWakeWordModels> GetModelsAsync(IAssetReader assetReader)
    {
        Asserts.IsNotNull(Model);

        var melspectrogramModel = new MelspectrogramModel(await assetReader.ReadBytesAsync("melspectrogram.onnx"));
        var embeddingModel = new EmbeddingModel(await assetReader.ReadBytesAsync("embedding_model.onnx"));
        var wakeWordModel = new WakeWordModel(await assetReader.ReadBytesAsync(GetWakeModelFile(Model!)));

        return new OpenWakeWordModels(embeddingModel, melspectrogramModel, wakeWordModel);
    }

    public WakeSettings ToSatelliteSettings()
    {
        return new WakeSettings()
        {
            MaxPatience = MaxPatience,
            PredictionThreshold = PredictionThreshold,
            Name = Model,
            RefractorySeconds = RefractorySeconds,
        };
    }

    private static string GetWakeModelFile(string model)
    {
        return model switch
        {
            "alexa" => "alexa_v0.1.onnx",
            _ => throw new NotImplementedException(),
        };
    }
}
