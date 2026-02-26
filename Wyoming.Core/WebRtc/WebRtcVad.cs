using Wyoming.Net.Core.WebRtc.Vad;
using Wyoming.Net.Core.WebRtc.Vad.Internal;

namespace Wyoming.Net.Core.WebRtc;

/// <summary>
/// Voice Activity Detection (VAD) instance.
/// Based on WebRTC's VAD engine, ported from libfvad.
/// </summary>
/// <remarks>
/// This class is not thread-safe. Each thread should use its own instance.
/// No unmanaged resources are held; disposal is not required.
/// </remarks>
public sealed class WebRtcVad
{
    private static readonly int[] ValidRatesKhz = { 8, 16, 32, 48 };

    private static readonly int[] ValidFrameTimesMs = { 10, 20, 30 };

    private readonly VadInst core = new();
    private int rateIdx;

    /// <summary>
    /// Creates and initializes a new VAD instance with default settings
    /// (Quality mode, 8000 Hz sample rate).
    /// </summary>
    public WebRtcVad()
    {
        Reset();
    }

    /// <summary>
    /// Reinitializes the VAD instance, clearing all state and resetting
    /// mode and sample rate to defaults.
    /// </summary>
    public void Reset()
    {
        VadCore.InitCore(core);
        rateIdx = 0;
    }

    /// <summary>
    /// Gets or sets the VAD operating aggressiveness mode.
    /// A more aggressive mode is more restrictive in reporting speech.
    /// Default is <see cref="VadMode.Quality"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The specified mode is not a valid <see cref="VadMode"/> value.</exception>
    public VadMode Mode
    {
        get => (VadMode)core.Total[0] switch
        {
            // Reverse-identify mode from the global threshold at index 0
            _ when core.Total[0] == 57 => VadMode.Quality,
            _ when core.Total[0] == 100 => VadMode.LowBitrate,
            _ when core.Total[0] == 285 => VadMode.Aggressive,
            _ when core.Total[0] == 1100 => VadMode.VeryAggressive,
            _ => VadMode.Quality,
        };
        set
        {
            int mode = (int)value;
            if (VadCore.SetModeCore(core, mode) != 0)
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    "Invalid VAD mode. Valid modes are Quality (0), LowBitrate (1), Aggressive (2), VeryAggressive (3).");
        }
    }

    /// <summary>
    /// Gets or sets the input sample rate in Hz.
    /// Valid values are 8000, 16000, 32000, and 48000. Default is 8000.
    /// Internally all processing is done at 8000 Hz; higher sample rates are downsampled first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The specified sample rate is not supported.</exception>
    public int SampleRate
    {
        get => ValidRatesKhz[rateIdx] * 1000;
        set
        {
            for (int i = 0; i < ValidRatesKhz.Length; i++)
            {
                if (ValidRatesKhz[i] * 1000 == value)
                {
                    rateIdx = i;
                    return;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Invalid sample rate. Valid rates are 8000, 16000, 32000, 48000.");
        }
    }

    /// <summary>
    /// Calculates a VAD decision for an audio frame.
    /// Only frames with a length of 10, 20, or 30 ms are supported.
    /// For example, at 8 kHz the frame length must be 80, 160, or 240 samples.
    /// </summary>
    /// <param name="frame">Audio frame of signed 16-bit samples.</param>
    /// <returns><c>true</c> if voice activity is detected; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentException">The frame length is not valid for the current sample rate.</exception>
    public bool Process(ReadOnlySpan<short> frame)
    {
        if (!IsValidLength(rateIdx, frame.Length))
            throw new ArgumentException(
                $"Invalid frame length {frame.Length} for sample rate {SampleRate} Hz. " +
                $"Valid lengths are 10, 20, or 30 ms frames.",
                nameof(frame));

        int rv = rateIdx switch
        {
            0 => VadCore.CalcVad8khz(core, frame, frame.Length),
            1 => VadCore.CalcVad16khz(core, frame, frame.Length),
            2 => VadCore.CalcVad32khz(core, frame, frame.Length),
            3 => VadCore.CalcVad48khz(core, frame, frame.Length),
            _ => throw new InvalidOperationException(),
        };

        return rv > 0;
    }

    private static bool IsValidLength(int rateIdx, int length)
    {
        int samplesPerMs = ValidRatesKhz[rateIdx];
        
        for (int i = 0; i < ValidFrameTimesMs.Length; i++)
        {
            if (ValidFrameTimesMs[i] * samplesPerMs == length)
                return true;
        }
        return false;
    }
}