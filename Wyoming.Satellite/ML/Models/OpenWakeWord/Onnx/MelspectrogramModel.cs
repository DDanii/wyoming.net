#if NET9_0_OR_GREATER

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Onnx;

public sealed class MelspectrogramModel : BaseModel, IMelspectrogramModel
{
    public MelspectrogramModel(byte[] model) : base(model)
    {
    }

    private static readonly long[] Shape = [1, 1760];

    public int FlattenedOutputSize => 256;

    public void GenerateSpectrogram(ReadOnlySpan<float> input, Span<float> destination)
    {
        // Android Encoding.PcmFloat delivers audio in [-1, 1].
        // The melspectrogram ONNX model expects int16-scale float values ([-32768, 32767]).
        // Scale up before inference so the mel bins land in the correct dynamic range.
        using var ortTensor = OrtValue.CreateAllocatedTensorValue(OrtAllocator.DefaultInstance, TensorElementType.Float, Shape);

        var tensorValue = ortTensor.GetTensorMutableDataAsSpan<float>();
        for (int i = 0; i < input.Length; i++)
        {
            tensorValue[i] = input[i] * 32768f;
        }

        var modelInput = new ModelInput("input", ortTensor);

        var result = session.Run(DefaultRunOptions, modelInput, session.OutputNames);
        using var modelOutput = new ModelOutput(result, Normalize);
    
        modelOutput.FlattenTo(destination);    
    }

    private static void Normalize(in Span<float> outputBuffer)
    {
        for (int i = 0; i < outputBuffer.Length; i++)
        {
            outputBuffer[i] = outputBuffer[i] / 10.0f + 2.0f;
        }
    }
}

#endif