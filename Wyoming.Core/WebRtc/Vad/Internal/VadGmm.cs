using System.Runtime.CompilerServices;

namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

internal static class VadGmm
{
    private const int CompVar = 22005;
    private const short Log2Exp = 5909; // log2(exp(1)) in Q12

    /// <summary>
    /// Calculates the probability for <paramref name="input"/> from a normal distribution
    /// N(<paramref name="mean"/>, <paramref name="std"/>).
    /// </summary>
    /// <param name="input">Input sample in Q4.</param>
    /// <param name="mean">Mean in Q7.</param>
    /// <param name="std">Standard deviation in Q7.</param>
    /// <param name="delta">Output: (input - mean) / std^2 in Q11.</param>
    /// <returns>Probability in Q20.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GaussianProbability(short input, short mean, short std, out short delta)
    {
        // inv_std = 1 / s, in Q10.  131072 = 1 in Q17.
        int tmp32 = 131072 + (std >> 1);
        short invStd = (short)SignalProcessing.DivW32W16(tmp32, std);

        // inv_std2 = 1 / s^2, in Q14.
        short tmp16 = (short)(invStd >> 2); // Q10 -> Q8
        short invStd2 = (short)((tmp16 * tmp16) >> 2); // Q14

        tmp16 = (short)(input << 3); // Q4 -> Q7
        tmp16 = (short)(tmp16 - mean); // Q7

        // delta = (x - m) / s^2, in Q11.
        delta = (short)((invStd2 * tmp16) >> 10);

        // exponent = (x - m)^2 / (2 * s^2), in Q10.
        tmp32 = (delta * tmp16) >> 9;

        short expValue = 0;
        if (tmp32 < CompVar)
        {
            // log2(exp(1)) * tmp32, in Q10.
            tmp16 = (short)((Log2Exp * tmp32) >> 12);
            tmp16 = (short)-tmp16;
            expValue = (short)(0x0400 | (tmp16 & 0x03FF));
            tmp16 ^= unchecked((short)0xFFFF);
            tmp16 >>= 10;
            tmp16 += 1;
            expValue >>= tmp16;
        }

        // (1 / s) * exp(...), in Q20 = Q10 * Q10.
        return invStd * expValue;
    }
}
