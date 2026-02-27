namespace Wyoming.Net.Core.WebRtc.Vad;

/// <summary>
/// VAD operating aggressiveness mode.
/// A more aggressive (higher) mode is more restrictive in reporting speech.
/// The probability of being speech when the VAD returns true increases with
/// increasing mode, at the cost of higher missed detection rate.
/// </summary>
public enum VadMode
{
    Quality = 0,
    LowBitrate = 1,
    Aggressive = 2,
    VeryAggressive = 3,
}
