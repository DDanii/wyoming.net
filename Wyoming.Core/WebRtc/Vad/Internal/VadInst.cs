namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

/// <summary>
/// Internal VAD state, equivalent to the C VadInstT struct.
/// Allocated once per Fvad instance; no fields are heap-allocated in the hot path.
/// </summary>
internal sealed class VadInst
{
    public const int NumChannels = 6;
    public const int NumGaussians = 2;
    public const int TableSize = NumChannels * NumGaussians;
    public const int MinEnergy = 10;

    public int Vad;
    public readonly int[] DownsamplingFilterStates = new int[4];
    public readonly State48KhzTo8Khz State48To8 = new();

    public readonly short[] NoiseMeans = new short[TableSize];
    public readonly short[] SpeechMeans = new short[TableSize];
    public readonly short[] NoiseStds = new short[TableSize];
    public readonly short[] SpeechStds = new short[TableSize];

    public int FrameCounter;
    public short OverHang;
    public short NumOfSpeech;

    public readonly short[] IndexVector = new short[16 * NumChannels];
    public readonly short[] LowValueVector = new short[16 * NumChannels];
    public readonly short[] MeanValue = new short[NumChannels];

    public readonly short[] UpperState = new short[5];
    public readonly short[] LowerState = new short[5];
    public readonly short[] HpFilterState = new short[4];

    public readonly short[] OverHangMax1 = new short[3];
    public readonly short[] OverHangMax2 = new short[3];
    public readonly short[] Individual = new short[3];
    public readonly short[] Total = new short[3];

    public readonly short[] FeatureVector = new short[NumChannels];
    public short TotalPower;

    public int InitFlag;
}
