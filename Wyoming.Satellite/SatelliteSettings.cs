using Wyoming.Net.Core.WebRtc.Vad;

namespace Wyoming.Net.Satellite;


public sealed record MicSettings 
{
    // public double VolumeMultiplier { get; init; } = 1.0;
    //
    // public int AutoGain { get; init; } = 0;
    //
    // public int NoiseSuppression { get; init; } = 0;

    public const int Rate = 16000;

    public const int Width = sizeof(float);

    public const int Channels = 1;

    public const int SamplesPerChunk = 1280;

    //
    // public bool MuteDuringAwakeWav { get; init; } = true;
    //
    // public double SecondsToMuteAfterAwakeWav { get; init; } = 0.5;
    //
    // public int? ChannelIndex { get; init; }
    //
    // public bool NeedsWebRtc => AutoGain > 0 || NoiseSuppression > 0;
    //
    // public bool NeedsProcessing => VolumeMultiplier != 1.0 || NeedsWebRtc;
}

public sealed record SndSettings
{
    public double VolumeMultiplier { get; init; } = 1.0;

    public string? AwakeWav { get; init; }

    public string? DoneWav { get; init; }

    public bool NeedsProcessing => Enabled && VolumeMultiplier != 1.0;

    public bool Enabled { get; set; } = true;
}

public sealed record WakeSettings
{
    public string? Name { get; set; }

    public int? RefractorySeconds { get; set; } = 5;

    public int MaxPatience { get; set; } = 15;

    public float PredictionThreshold { get; set; } = 0.5f;
}

public sealed record VadSettings
{
    [Flags]
    public enum VadType
    {
        WebRtc = 0,
        Silero = 1 << 0 //TODO: implement
    }
    
    public bool Enabled { get; set; } = false;

    public VadType Type { get; set; } = VadType.WebRtc;

    public VadMode WebRtcMode { get; set; } = VadMode.VeryAggressive;

    public bool UseEnergyGate { get; set; } = false;

    public float EnergyGateThreshold { get; set; } = 0.0008f;

    public float EnergyGateZcr { get; set; } = 0.4f;
}

public sealed record SatelliteSettings
{
    public static readonly MicSettings Mic = new();

    public static readonly VadSettings Vad = new();

    public static readonly WakeSettings Wake = new();

    public SndSettings Snd { get; init; } = new();
}
