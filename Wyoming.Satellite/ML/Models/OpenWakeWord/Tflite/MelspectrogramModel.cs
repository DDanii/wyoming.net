#if ANDROID

using Java.Nio;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tflite;

public sealed class MelspectrogramModel : BaseModel, IMelspectrogramModel
{
    private const int InputSize = 1760;
    private const int OutputSize = 256;

    // Staging arrays cross the Java/C# boundary — allocated once, reused every call.
    private readonly float[] inputStaging = new float[InputSize];
    private readonly float[] outputStaging = new float[OutputSize];

    // Direct native ByteBuffers — TFLite reads/writes them without an extra copy.
    private readonly ByteBuffer inputByteBuffer;
    private readonly ByteBuffer outputByteBuffer;

    // FloatBuffer views share the underlying native memory of their ByteBuffers.
    private readonly FloatBuffer inputFloatBuffer;
    private readonly FloatBuffer outputFloatBuffer;

    public MelspectrogramModel(byte[] model) : base(model)
    {
        inputByteBuffer = ByteBuffer.AllocateDirect(InputSize * sizeof(float))!;
        inputByteBuffer.Order(ByteOrder.NativeOrder()!);
        inputFloatBuffer = inputByteBuffer.AsFloatBuffer()!;

        outputByteBuffer = ByteBuffer.AllocateDirect(OutputSize * sizeof(float))!;
        outputByteBuffer.Order(ByteOrder.NativeOrder()!);
        outputFloatBuffer = outputByteBuffer.AsFloatBuffer()!;

        interpreter.ResizeInput(0, [1, InputSize]);
        interpreter.AllocateTensors();
    }

    public int FlattenedOutputSize => OutputSize;

    public void GenerateSpectrogram(ReadOnlySpan<float> input, Span<float> destination)
    {
        // Android Encoding.PcmFloat delivers audio in [-1, 1].
        // The melspectrogram model expects int16-scale float values ([-32768, 32767]).
        for (int i = 0; i < input.Length; i++)
        {
            inputStaging[i] = input[i] * 32768f;
        }
        inputFloatBuffer.Rewind();
        inputFloatBuffer.Put(inputStaging);

        // ByteBuffer position is independent of FloatBuffer; rewind both sides.
        inputByteBuffer.Rewind();
        outputByteBuffer.Rewind();

        interpreter.Run(inputByteBuffer, outputByteBuffer);

        // Read results from native output buffer into staging, then to destination.
        outputFloatBuffer.Rewind();
        outputFloatBuffer.Get(outputStaging);
        outputStaging.AsSpan().CopyTo(destination);

        Normalize(destination.Slice(0, OutputSize));
    }

    private static void Normalize(Span<float> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = buffer[i] / 10.0f + 2.0f;
        }
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
