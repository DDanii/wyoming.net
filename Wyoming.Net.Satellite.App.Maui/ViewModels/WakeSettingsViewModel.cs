using CommunityToolkit.Mvvm.ComponentModel;
using Wyoming.Net.Core;
using Wyoming.Net.Satellite.App.Maui.Abstractions;
#if ANDROID
using Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tflite;
#else
using Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Onnx;
#endif
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

        var embeddingModel = new EmbeddingModel(await assetReader.ReadBytesAsync($"embedding_model{GetModelExtension()}"));
        var melspectrogramModel = new MelspectrogramModel(await assetReader.ReadBytesAsync($"melspectrogram{GetModelExtension()}"));
        
        var wakeWordModel = new WakeWordModel(await assetReader.ReadBytesAsync(GetWakeModelFile(Model!)));

        return new OpenWakeWordModels(embeddingModel, melspectrogramModel, wakeWordModel);
    }

    private static string GetWakeModelFile(string model)
    {
        return model switch
        {
            "alexa" => $"alexa_v0.1{GetModelExtension()}",
            _ => throw new NotImplementedException(),
        };
    }

    private static string GetModelExtension()
    {
        #if ANDROID
        // Note: the android version of melspectrogram is a custom version I made with fixed size inputs
        // the same used for Tizen.
        // The reason is that tflite java wrapper implementation does an eager call to AllocateTensors
        // before we can call resize inputs, which causes the interpreter to fail
        return ".tflite";
        #else
        return ".onnx";
        #endif
    }
}
