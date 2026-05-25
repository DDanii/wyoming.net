#if TIZEN8_0_OR_GREATER

using System.Runtime.InteropServices;
using Tizen.MachineLearning.Inference;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tizen;

public sealed class MelspectrogramModel : TizenModel, IMelspectrogramModel
{
    private readonly byte[] _inputBuffer;
    private readonly TensorsData _tensorData;

    public MelspectrogramModel(string modelPath) : base(modelPath)
    {
        _inputBuffer = new byte[(1280 + 480) * sizeof(float)];
        _tensorData = engine.Input.GetTensorsData();
    }

    public int FlattenedOutputSize => 256;

    public void GenerateSpectrogram(ReadOnlySpan<float> input, Span<float> destination)
    {
        // The model expects int16-scale float values ([-32768, 32767]).
        // Input arrives as normalized [-1, 1] floats, so scale up before inference.
        var inputFloats = MemoryMarshal.Cast<byte, float>(_inputBuffer);

        for (int i = 0; i < input.Length; i++)
        {
            inputFloats[i] = input[i] * 32768f;
        }

        _tensorData.SetTensorData(0, _inputBuffer);

        using var outData = engine.Invoke(_tensorData);
        var bytesOut = outData.GetTensorData(0);

        MemoryMarshal.Cast<byte, float>(bytesOut).CopyTo(destination);
        Normalize(destination);
    }

    private static void Normalize(Span<float> outputBuffer)
    {
        for (int i = 0; i < outputBuffer.Length; i++)
        {
            outputBuffer[i] = outputBuffer[i] / 10.0f + 2.0f;
        }
    }
}

#endif