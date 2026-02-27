using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

internal static class VadFilterbank
{
    private const short LogConst = 24660;        // 160*log10(2) in Q9
    private const short LogEnergyIntPart = 14336; // 14 in Q10

    private static ReadOnlySpan<short> HpZeroCoefs => new short[] { 6631, -13262, 6631 };
    private static ReadOnlySpan<short> HpPoleCoefs => new short[] { 16384, -7756, 5620 };

    // Allpass filter coefficients, upper and lower, in Q15. Upper: 0.64, Lower: 0.17
    private static ReadOnlySpan<short> AllPassCoefsQ15 => new short[] { 20972, 5571 };

    private static ReadOnlySpan<short> OffsetVector => new short[] { 368, 368, 272, 176, 176, 176 };

    /// <summary>
    /// High pass filtering with a cut-off frequency at 80 Hz (data sampled at 500 Hz).
    /// </summary>
    private static void HighPassFilter(ReadOnlySpan<short> dataIn, int dataLength,
        Span<short> filterState, Span<short> dataOut)
    {
        int zc0 = 6631, zc1 = -13262, zc2 = 6631;
        int pc1 = -7756, pc2 = 5620;

        ref short inRef = ref MemoryMarshal.GetReference(dataIn);
        ref short outRef = ref MemoryMarshal.GetReference(dataOut);
        ref short fs = ref MemoryMarshal.GetReference(filterState);

        short fs0 = fs, fs1 = Unsafe.Add(ref fs, 1);
        short fs2 = Unsafe.Add(ref fs, 2), fs3 = Unsafe.Add(ref fs, 3);

        for (int i = 0; i < dataLength; i++)
        {
            short inVal = Unsafe.Add(ref inRef, i);

            int tmp32 = zc0 * inVal + zc1 * fs0 + zc2 * fs1;
            fs1 = fs0;
            fs0 = inVal;

            tmp32 -= pc1 * fs2;
            tmp32 -= pc2 * fs3;
            fs3 = fs2;
            fs2 = (short)(tmp32 >> 14);
            Unsafe.Add(ref outRef, i) = fs2;
        }

        fs = fs0; Unsafe.Add(ref fs, 1) = fs1;
        Unsafe.Add(ref fs, 2) = fs2; Unsafe.Add(ref fs, 3) = fs3;
    }

    /// <summary>
    /// All pass filtering of data, used before splitting into two frequency bands.
    /// Input is read with stride 2 (every other sample).
    /// </summary>
    private static void AllPassFilter(ReadOnlySpan<short> dataIn, int dataLength,
        short filterCoefficient, ref short filterState, Span<short> dataOut)
    {
        int state32 = filterState << 16;
        int coef = filterCoefficient;
        ref short inRef = ref MemoryMarshal.GetReference(dataIn);
        ref short outRef = ref MemoryMarshal.GetReference(dataOut);

        for (int i = 0; i < dataLength; i++)
        {
            int inSample = Unsafe.Add(ref inRef, i * 2);
            int tmp32 = state32 + coef * inSample;
            short tmp16 = (short)(tmp32 >> 16);
            Unsafe.Add(ref outRef, i) = tmp16;
            state32 = (inSample << 14) - coef * tmp16;
            state32 <<= 1;
        }

        filterState = (short)(state32 >> 16);
    }

    /// <summary>
    /// Splits data into upper (high pass) and lower (low pass) frequency bands.
    /// Output length = dataLength / 2 each.
    /// </summary>
    [SkipLocalsInit]
    private static void SplitFilter(ReadOnlySpan<short> dataIn, int dataLength,
        ref short upperState, ref short lowerState,
        Span<short> hpDataOut, Span<short> lpDataOut)
    {
        int halfLength = dataLength >> 1;
        ReadOnlySpan<short> ap = AllPassCoefsQ15;

        AllPassFilter(dataIn, halfLength, ap[0], ref upperState, hpDataOut);
        AllPassFilter(dataIn[1..], halfLength, ap[1], ref lowerState, lpDataOut);

        int i = 0;
        
#if NET9_0_OR_GREATER
        if (Vector128.IsHardwareAccelerated && halfLength >= Vector128<short>.Count)
        {
            ref short hpRef = ref MemoryMarshal.GetReference(hpDataOut);
            ref short lpRef = ref MemoryMarshal.GetReference(lpDataOut);
            for (; i <= halfLength - Vector128<short>.Count; i += Vector128<short>.Count)
            {
                var hp = Vector128.LoadUnsafe(ref hpRef, (nuint)i);
                var lp = Vector128.LoadUnsafe(ref lpRef, (nuint)i);
                Vector128.StoreUnsafe(hp - lp, ref hpRef, (nuint)i);
                Vector128.StoreUnsafe(lp + hp, ref lpRef, (nuint)i);
            }
        }

#endif
        for (; i < halfLength; i++)
        {
            short tmpOut = hpDataOut[i];
            hpDataOut[i] = (short)(hpDataOut[i] - lpDataOut[i]);
            lpDataOut[i] = (short)(lpDataOut[i] + tmpOut);
        }
    }

    /// <summary>
    /// Calculates the energy of data in dB (Q4), and updates total_energy if needed.
    /// </summary>
    private static void LogOfEnergy(ReadOnlySpan<short> dataIn, int dataLength,
        short offset, ref short totalEnergy, out short logEnergy)
    {
        int totRshifts = 0;
        uint energy = (uint)SignalProcessing.Energy(dataIn[..dataLength], out totRshifts);

        if (energy != 0)
        {
            int normalizingRshifts = 17 - SignalProcessing.NormU32(energy);
            short log2Energy = LogEnergyIntPart;

            totRshifts += normalizingRshifts;
            if (normalizingRshifts < 0)
                energy <<= -normalizingRshifts;
            else
                energy >>= normalizingRshifts;

            log2Energy += (short)((energy & 0x00003FFF) >> 4);

            logEnergy = (short)(((LogConst * log2Energy) >> 19) +
                ((totRshifts * LogConst) >> 9));

            if (logEnergy < 0)
                logEnergy = 0;
        }
        else
        {
            logEnergy = offset;
            return;
        }

        logEnergy += offset;

        if (totalEnergy <= VadInst.MinEnergy)
        {
            if (totRshifts >= 0)
            {
                totalEnergy += VadInst.MinEnergy + 1;
            }
            else
            {
                totalEnergy += (short)(energy >> -totRshifts);
            }
        }
    }

    /// <summary>
    /// Takes data_length samples and calculates log10(energy) in each of the 6 frequency bands.
    /// Returns approximate total energy.
    /// </summary>
    [SkipLocalsInit]
    public static short CalculateFeatures(VadInst self, ReadOnlySpan<short> dataIn,
        int dataLength, Span<short> features)
    {
        short totalEnergy = 0;

        Debug.Assert(dataLength <= 240);

        Span<short> hp120 = stackalloc short[120];
        Span<short> lp120 = stackalloc short[120];
        Span<short> hp60 = stackalloc short[60];
        Span<short> lp60 = stackalloc short[60];

        int halfDataLength = dataLength >> 1;
        int length = halfDataLength;
        ReadOnlySpan<short> offsets = OffsetVector;

        // Split at 2000 Hz and downsample. [0-4000] -> [2000-4000] + [0-2000]
        SplitFilter(dataIn, dataLength,
            ref self.UpperState[0], ref self.LowerState[0], hp120, lp120);

        // Split [2000-4000] at 3000 Hz. -> [3000-4000] + [2000-3000]
        SplitFilter(hp120[..length], length,
            ref self.UpperState[1], ref self.LowerState[1], hp60, lp60);

        length >>= 1;

        // Energy in 3000 Hz - 4000 Hz
        LogOfEnergy(hp60, length, offsets[5], ref totalEnergy, out features[5]);

        // Energy in 2000 Hz - 3000 Hz
        LogOfEnergy(lp60, length, offsets[4], ref totalEnergy, out features[4]);

        // Split [0-2000] at 1000 Hz. -> [1000-2000] + [0-1000]
        length = halfDataLength;
        SplitFilter(lp120[..length], length,
            ref self.UpperState[2], ref self.LowerState[2], hp60, lp60);

        length >>= 1;

        // Energy in 1000 Hz - 2000 Hz
        LogOfEnergy(hp60, length, offsets[3], ref totalEnergy, out features[3]);

        // Split [0-1000] at 500 Hz. -> [500-1000] + [0-500]
        SplitFilter(lp60[..length], length,
            ref self.UpperState[3], ref self.LowerState[3], hp120, lp120);

        length >>= 1;

        // Energy in 500 Hz - 1000 Hz
        LogOfEnergy(hp120, length, offsets[2], ref totalEnergy, out features[2]);

        // Split [0-500] at 250 Hz. -> [250-500] + [0-250]
        SplitFilter(lp120[..length], length,
            ref self.UpperState[4], ref self.LowerState[4], hp60, lp60);

        length >>= 1;

        // Energy in 250 Hz - 500 Hz
        LogOfEnergy(hp60, length, offsets[1], ref totalEnergy, out features[1]);

        // Remove 0 Hz - 80 Hz by high pass filtering the lower band
        HighPassFilter(lp60, length, self.HpFilterState, hp120);

        // Energy in 80 Hz - 250 Hz
        LogOfEnergy(hp120, length, offsets[0], ref totalEnergy, out features[0]);

        return totalEnergy;
    }
}
