#if ANDROID

using Java.Nio;
using Xamarin.TensorFlow.Lite;
using Xamarin.TensorFlow.Lite.GPU;
using Xamarin.TensorFlow.Lite.Nnapi;

namespace Wyoming.Net.Satellite.ML.Models.OpenWakeWord.Tflite;

public abstract class BaseModel : IDisposable
{
    private static readonly bool GpuSupported;

    static BaseModel()
    {
        using var compat = new CompatibilityList();
        GpuSupported = compat.IsDelegateSupportedOnThisDevice;
    }

    protected readonly Interpreter interpreter;

    protected BaseModel(byte[] model)
    {
        var buffer = ByteBuffer.AllocateDirect(model.Length)!;
        buffer.Order(ByteOrder.NativeOrder()!);
        buffer.Put(model);
        buffer.Rewind();
        
        var options = new Interpreter.Options();
        options.AddDelegate(new NnApiDelegate());

        if (GpuSupported)
        {
            options.AddDelegate(new GpuDelegate());
        }
        
        interpreter = new Interpreter(buffer, options);
    }

    ~BaseModel()
    {
        Dispose();
    }

    public virtual void Dispose()
    {
        interpreter.Dispose();
        GC.SuppressFinalize(this);
    }
}

#endif
