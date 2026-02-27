using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

internal static class VadSp
{
    private const int AllPassCoefQ13_0 = 5243;
    private const int AllPassCoefQ13_1 = 1392;
    private const short SmoothingDown = 6553;   // 0.2 in Q15
    private const short SmoothingUp = 32439;    // 0.99 in Q15

    /// <summary>
    /// Downsamples the signal by a factor of 2 using allpass filters.
    /// </summary>
    [SkipLocalsInit]
    public static void Downsampling(ReadOnlySpan<short> signalIn, Span<short> signalOut,
        Span<int> filterState)
    {
        int tmp32_1 = filterState[0];
        int tmp32_2 = filterState[1];
        int halfLength = signalIn.Length >> 1;

        ref short inRef = ref MemoryMarshal.GetReference(signalIn);
        ref short outRef = ref MemoryMarshal.GetReference(signalOut);

        for (int n = 0; n < halfLength; n++)
        {
            int idx = n << 1;
            int in0 = Unsafe.Add(ref inRef, idx);
            int in1 = Unsafe.Add(ref inRef, idx + 1);

            short tmp16_1 = (short)((tmp32_1 >> 1) + ((AllPassCoefQ13_0 * in0) >> 14));
            tmp32_1 = in0 - ((AllPassCoefQ13_0 * tmp16_1) >> 12);

            short tmp16_2 = (short)((tmp32_2 >> 1) + ((AllPassCoefQ13_1 * in1) >> 14));
            tmp32_2 = in1 - ((AllPassCoefQ13_1 * tmp16_2) >> 12);

            Unsafe.Add(ref outRef, n) = (short)(tmp16_1 + tmp16_2);
        }

        filterState[0] = tmp32_1;
        filterState[1] = tmp32_2;
    }

    /// <summary>
    /// Tracks the 16 smallest values over a 100-frame window and returns a smoothed median.
    /// </summary>
    public static short FindMinimum(VadInst self, short featureValue, int channel)
    {
        int offset = channel << 4;
        short currentMedian = 1600;
        short alpha = 0;

        Span<short> age = self.IndexVector.AsSpan(offset, 16);
        Span<short> smallestValues = self.LowValueVector.AsSpan(offset, 16);

        // Age each value and remove those that are too old.
        for (int i = 0; i < 16; i++)
        {
            if (age[i] != 100)
            {
                age[i]++;
            }
            else
            {
                for (int j = i; j < 15; j++)
                {
                    smallestValues[j] = smallestValues[j + 1];
                    age[j] = age[j + 1];
                }
                age[15] = 101;
                smallestValues[15] = 10000;
            }
        }

        // Binary-search-like insertion position finding.
        int position = -1;
        if (featureValue < smallestValues[7])
        {
            if (featureValue < smallestValues[3])
            {
                if (featureValue < smallestValues[1])
                    position = featureValue < smallestValues[0] ? 0 : 1;
                else
                    position = featureValue < smallestValues[2] ? 2 : 3;
            }
            else if (featureValue < smallestValues[5])
            {
                position = featureValue < smallestValues[4] ? 4 : 5;
            }
            else
            {
                position = featureValue < smallestValues[6] ? 6 : 7;
            }
        }
        else if (featureValue < smallestValues[15])
        {
            if (featureValue < smallestValues[11])
            {
                if (featureValue < smallestValues[9])
                    position = featureValue < smallestValues[8] ? 8 : 9;
                else
                    position = featureValue < smallestValues[10] ? 10 : 11;
            }
            else if (featureValue < smallestValues[13])
            {
                position = featureValue < smallestValues[12] ? 12 : 13;
            }
            else
            {
                position = featureValue < smallestValues[14] ? 14 : 15;
            }
        }

        // Insert at position and shift larger values up.
        if (position > -1)
        {
            for (int i = 15; i > position; i--)
            {
                smallestValues[i] = smallestValues[i - 1];
                age[i] = age[i - 1];
            }
            smallestValues[position] = featureValue;
            age[position] = 1;
        }

        if (self.FrameCounter > 2)
            currentMedian = smallestValues[2];
        else if (self.FrameCounter > 0)
            currentMedian = smallestValues[0];

        if (self.FrameCounter > 0)
        {
            alpha = currentMedian < self.MeanValue[channel] ? SmoothingDown : SmoothingUp;
        }

        int tmp32 = (alpha + 1) * self.MeanValue[channel];
        tmp32 += (SignalProcessing.Word16Max - alpha) * currentMedian;
        tmp32 += 16384;
        self.MeanValue[channel] = (short)(tmp32 >> 15);

        return self.MeanValue[channel];
    }
}
