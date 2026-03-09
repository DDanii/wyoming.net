#if ANDROID

using Java.Nio;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tflite;

public sealed class EmbeddingModel : BaseModel, IEmbeddingModel
{
    // Shape [1, 76, 32, 1]
    public int FlatShapeSize => 1 * 76 * 32 * 1;
    public int FlattenedOutputSize => 96;

    private readonly float[] inputStaging;
    private readonly float[] outputStaging = new float[96];

    private readonly ByteBuffer inputByteBuffer;
    private readonly ByteBuffer outputByteBuffer;

    private readonly FloatBuffer inputFloatBuffer;
    private readonly FloatBuffer outputFloatBuffer;

    public EmbeddingModel(byte[] model) : base(model)
    {
        inputStaging = new float[FlatShapeSize];

        inputByteBuffer = ByteBuffer.AllocateDirect(FlatShapeSize * sizeof(float))!;
        inputByteBuffer.Order(ByteOrder.NativeOrder()!);
        inputFloatBuffer = inputByteBuffer.AsFloatBuffer()!;

        outputByteBuffer = ByteBuffer.AllocateDirect(FlattenedOutputSize * sizeof(float))!;
        outputByteBuffer.Order(ByteOrder.NativeOrder()!);
        outputFloatBuffer = outputByteBuffer.AsFloatBuffer()!;
    }

    public void GenerateAudioEmbeddings(ReadOnlySpan<float> input, Span<float> destination)
    {
        input.CopyTo(inputStaging);
        inputFloatBuffer.Rewind();
        inputFloatBuffer.Put(inputStaging);

        inputByteBuffer.Rewind();
        outputByteBuffer.Rewind();

        interpreter.Run(inputByteBuffer, outputByteBuffer);

        outputFloatBuffer.Rewind();
        outputFloatBuffer.Get(outputStaging);
        outputStaging.AsSpan().CopyTo(destination);
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
