using System.Runtime.CompilerServices;
// ReSharper disable InconsistentNaming
// ReSharper disable UseCollectionExpression - Compatibility with net10

namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

internal sealed class State48KhzTo8Khz
{
    public readonly int[] S_48_24 = new int[8];
    public readonly int[] S_24_24 = new int[16];
    public readonly int[] S_24_16 = new int[8];
    public readonly int[] S_16_8 = new int[8];

    public void Reset()
    {
        Array.Clear(S_48_24);
        Array.Clear(S_24_24);
        Array.Clear(S_24_16);
        Array.Clear(S_16_8);
    }
}

internal static class Resampler
{
    private static ReadOnlySpan<short> ResampleAllpass0 => new short[] { 821, 6110, 12382 };
    private static ReadOnlySpan<short> ResampleAllpass1 => new short[] { 3050, 9368, 15063 };

    private static ReadOnlySpan<short> Coefficients48To32_0 =>
        new short[] { 778, -2050, 1087, 23285, 12903, -3783, 441, 222 };

    private static ReadOnlySpan<short> Coefficients48To32_1 =>
        new short[] { 222, 441, -3783, 12903, 23285, 1087, -2050, 778 };

    [SkipLocalsInit]
    public static void Resample48khzTo8khz(ReadOnlySpan<short> input, Span<short> output,
        State48KhzTo8Khz state, Span<int> tmpmem)
    {
        // 48 --> 24: int16 in[480] -> int32 out[240]
        DownBy2ShortToInt(input[..480], tmpmem.Slice(256, 240), state.S_48_24);

        // 24 --> 24(LP): int32 in[240] -> int32 out[240]
        LPBy2IntToInt(tmpmem.Slice(256, 240), tmpmem.Slice(16, 240), state.S_24_24);

        // 24 --> 16: copy state to/from input array, then resample
        state.S_24_16.AsSpan().CopyTo(tmpmem.Slice(8, 8));
        tmpmem.Slice(248, 8).CopyTo(state.S_24_16);
        Resample48khzTo32khz(tmpmem.Slice(8, 248), tmpmem[..160], 80);

        // 16 --> 8: int32 in[160] -> int16 out[80]
        DownBy2IntToShort(tmpmem[..160], output[..80], state.S_16_8);
    }

    [SkipLocalsInit]
    public static void Resample48khzTo32khz(ReadOnlySpan<int> input, Span<int> output, int k)
    {
        ReadOnlySpan<short> c0 = Coefficients48To32_0;
        ReadOnlySpan<short> c1 = Coefficients48To32_1;
        int inIdx = 0;
        int outIdx = 0;

        for (int m = 0; m < k; m++)
        {
            int tmp = 1 << 14;
            tmp += c0[0] * input[inIdx];
            tmp += c0[1] * input[inIdx + 1];
            tmp += c0[2] * input[inIdx + 2];
            tmp += c0[3] * input[inIdx + 3];
            tmp += c0[4] * input[inIdx + 4];
            tmp += c0[5] * input[inIdx + 5];
            tmp += c0[6] * input[inIdx + 6];
            tmp += c0[7] * input[inIdx + 7];
            output[outIdx] = tmp;

            tmp = 1 << 14;
            tmp += c1[0] * input[inIdx + 1];
            tmp += c1[1] * input[inIdx + 2];
            tmp += c1[2] * input[inIdx + 3];
            tmp += c1[3] * input[inIdx + 4];
            tmp += c1[4] * input[inIdx + 5];
            tmp += c1[5] * input[inIdx + 6];
            tmp += c1[6] * input[inIdx + 7];
            tmp += c1[7] * input[inIdx + 8];
            output[outIdx + 1] = tmp;

            inIdx += 3;
            outIdx += 2;
        }
    }

    /// <summary>
    /// Decimator: int16 input -> int32 output (shifted left 15, + offset 16384), length halved.
    /// </summary>
    [SkipLocalsInit]
    public static void DownBy2ShortToInt(ReadOnlySpan<short> input, Span<int> output, Span<int> state)
    {
        unchecked
        {
            ReadOnlySpan<short> ap1 = ResampleAllpass1;
            ReadOnlySpan<short> ap0 = ResampleAllpass0;
            int len = input.Length >> 1;

            // lower allpass filter (even input samples)
            for (int i = 0; i < len; i++)
            {
                int tmp0 = ((int)input[i << 1] << 15) + (1 << 14);
                int diff = tmp0 - state[1];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[0] + diff * ap1[0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[1] + diff * ap1[1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[3] = state[2] + diff * ap1[2];
                state[2] = tmp0;

                output[i] = state[3] >> 1;
            }

            // upper allpass filter (odd input samples)
            for (int i = 0; i < len; i++)
            {
                int tmp0 = ((int)input[(i << 1) + 1] << 15) + (1 << 14);
                int diff = tmp0 - state[5];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[4] + diff * ap0[0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[5] + diff * ap0[1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[7] = state[6] + diff * ap0[2];
                state[6] = tmp0;

                output[i] += state[7] >> 1;
            }
        }
    }

    /// <summary>
    /// Decimator: int32 input (shifted left 15, + offset 16384) -> int16 output (saturated), length halved.
    /// </summary>
    [SkipLocalsInit]
    public static void DownBy2IntToShort(Span<int> input, Span<short> output, Span<int> state)
    {
        unchecked
        {
            ReadOnlySpan<short> ap1 = ResampleAllpass1;
            ReadOnlySpan<short> ap0 = ResampleAllpass0;
            int len = input.Length >> 1;

            // lower allpass filter (even input samples)
            for (int i = 0; i < len; i++)
            {
                int tmp0 = input[i << 1];
                int diff = tmp0 - state[1];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[0] + diff * ap1[0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[1] + diff * ap1[1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[3] = state[2] + diff * ap1[2];
                state[2] = tmp0;

                input[i << 1] = state[3] >> 1;
            }

            // upper allpass filter (odd input samples)
            for (int i = 0; i < len; i++)
            {
                int tmp0 = input[(i << 1) + 1];
                int diff = tmp0 - state[5];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[4] + diff * ap0[0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[5] + diff * ap0[1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[7] = state[6] + diff * ap0[2];
                state[6] = tmp0;

                input[(i << 1) + 1] = state[7] >> 1;
            }

            // combine allpass outputs
            for (int i = 0; i < len; i += 2)
            {
                int tmp0 = (input[i << 1] + input[(i << 1) + 1]) >> 15;
                int tmp1 = (input[(i << 1) + 2] + input[(i << 1) + 3]) >> 15;
                if (tmp0 > 0x00007FFF) tmp0 = 0x00007FFF;
                if (tmp0 < unchecked((int)0xFFFF8000)) tmp0 = unchecked((int)0xFFFF8000);
                output[i] = (short)tmp0;
                if (tmp1 > 0x00007FFF) tmp1 = 0x00007FFF;
                if (tmp1 < unchecked((int)0xFFFF8000)) tmp1 = unchecked((int)0xFFFF8000);
                output[i + 1] = (short)tmp1;
            }
        }
    }

    /// <summary>
    /// Lowpass filter: int32 input (shifted left 15, + offset 16384) -> int32 output (normalized).
    /// State length = 16.
    /// </summary>
    [SkipLocalsInit]
    public static void LPBy2IntToInt(ReadOnlySpan<int> input, Span<int> output, Span<int> state)
    {
        unchecked
        {
            ReadOnlySpan<short> ap1 = ResampleAllpass1;
            ReadOnlySpan<short> ap0 = ResampleAllpass0;
            int len = input.Length >> 1;

            // lower allpass filter: odd input -> even output samples
            int tmp0 = state[12];
            for (int i = 0; i < len; i++)
            {
                int diff = tmp0 - state[1];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[0] + diff * ap1[0];
                state[0] = tmp0;
                diff = tmp1 - state[2];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[1] + diff * ap1[1];
                state[1] = tmp1;
                diff = tmp0 - state[3];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[3] = state[2] + diff * ap1[2];
                state[2] = tmp0;

                output[i << 1] = state[3] >> 1;
                tmp0 = input[(i << 1) + 1]; // odd input
            }

            // upper allpass filter: even input -> even output samples
            for (int i = 0; i < len; i++)
            {
                tmp0 = input[i << 1];
                int diff = tmp0 - state[5];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[4] + diff * ap0[0];
                state[4] = tmp0;
                diff = tmp1 - state[6];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[5] + diff * ap0[1];
                state[5] = tmp1;
                diff = tmp0 - state[7];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[7] = state[6] + diff * ap0[2];
                state[6] = tmp0;

                output[i << 1] = (output[i << 1] + (state[7] >> 1)) >> 15;
            }

            // lower allpass filter: even input -> odd output samples
            for (int i = 0; i < len; i++)
            {
                tmp0 = input[i << 1];
                int diff = tmp0 - state[9];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[8] + diff * ap1[0];
                state[8] = tmp0;
                diff = tmp1 - state[10];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[9] + diff * ap1[1];
                state[9] = tmp1;
                diff = tmp0 - state[11];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[11] = state[10] + diff * ap1[2];
                state[10] = tmp0;

                output[(i << 1) + 1] = state[11] >> 1;
            }

            // upper allpass filter: odd input -> odd output samples
            for (int i = 0; i < len; i++)
            {
                tmp0 = input[(i << 1) + 1];
                int diff = tmp0 - state[13];
                diff = (diff + (1 << 13)) >> 14;
                int tmp1 = state[12] + diff * ap0[0];
                state[12] = tmp0;
                diff = tmp1 - state[14];
                diff >>= 14;
                if (diff < 0) diff += 1;
                tmp0 = state[13] + diff * ap0[1];
                state[13] = tmp1;
                diff = tmp0 - state[15];
                diff >>= 14;
                if (diff < 0) diff += 1;
                state[15] = state[14] + diff * ap0[2];
                state[14] = tmp0;

                output[(i << 1) + 1] = (output[(i << 1) + 1] + (state[15] >> 1)) >> 15;
            }
        }
    }
}
