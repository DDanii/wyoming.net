#if ANDROID

using Java.Nio;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tflite;

public sealed class WakeWordModel : BaseModel, IWakeWordModel
{
    // Shape [1, 16, 96]
    public int FlatShapeSize => 1 * 16 * 96;

    private readonly float[] inputStaging;
    private readonly float[] outputStaging = new float[1];

    private readonly ByteBuffer inputByteBuffer;
    private readonly ByteBuffer outputByteBuffer;

    private readonly FloatBuffer inputFloatBuffer;
    private readonly FloatBuffer outputFloatBuffer;

    public WakeWordModel(byte[] model) : base(model)
    {
        inputStaging = new float[FlatShapeSize];

        inputByteBuffer = ByteBuffer.AllocateDirect(FlatShapeSize * sizeof(float))!;
        inputByteBuffer.Order(ByteOrder.NativeOrder()!);
        inputFloatBuffer = inputByteBuffer.AsFloatBuffer()!;

        outputByteBuffer = ByteBuffer.AllocateDirect(1 * sizeof(float))!;
        outputByteBuffer.Order(ByteOrder.NativeOrder()!);
        outputFloatBuffer = outputByteBuffer.AsFloatBuffer()!;
    }

    public float Predict(ReadOnlySpan<float> input)
    {
        input.CopyTo(inputStaging);
        inputFloatBuffer.Rewind();
        inputFloatBuffer.Put(inputStaging);

        inputByteBuffer.Rewind();
        outputByteBuffer.Rewind();

        interpreter.Run(inputByteBuffer, outputByteBuffer);

        outputFloatBuffer.Rewind();
        outputFloatBuffer.Get(outputStaging);
        return outputStaging[0];
    }

    public override void Dispose()
    {
        outputFloatBuffer.Dispose();
        outputByteBuffer.Dispose();
        inputFloatBuffer.Dispose();
        inputByteBuffer.Dispose();
        base.Dispose();
    }
}

#endif
