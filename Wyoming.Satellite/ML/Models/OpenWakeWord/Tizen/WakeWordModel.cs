#if TIZEN8_0_OR_GREATER
using System.Runtime.InteropServices;
using Tizen.MachineLearning.Inference;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tizen;

public sealed class WakeWordModel : TizenModel, IWakeWordModel
{
    private readonly byte[] _inputBuffer;
    private readonly TensorsData _tensorData;

    public WakeWordModel(string modelPath) : base(modelPath)
    {
        _inputBuffer = new byte[FlatShapeSize * sizeof(float)];
        _tensorData = engine.Input.GetTensorsData();
    }

    public int FlatShapeSize => 1 * 16 * 96;

    public float Predict(ReadOnlySpan<float> input)
    {
        var bytes = MemoryMarshal.Cast<float, byte>(input);
        bytes.CopyTo(_inputBuffer);

        _tensorData.SetTensorData(0, _inputBuffer);

        using var outData = engine.Invoke(_tensorData);
        var bytesOut = outData.GetTensorData(0);

        return BitConverter.ToSingle(bytesOut);
    }
}

#endif