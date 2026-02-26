using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

internal static class SignalProcessing
{
    public const short Word16Max = short.MaxValue;   // 32767
    public const short Word16Min = short.MinValue;   // -32768
    public const int Word32Max = int.MaxValue;       // 0x7FFFFFFF
    public const int Word32Min = int.MinValue;       // 0x80000000

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Mul(int a, int b) => a * b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLeadingZeros32(uint n)
    {
        return BitOperations.LeadingZeroCount(n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetSizeInBits(uint n)
    {
        return (short)(32 - CountLeadingZeros32(n));
    }

    /// <summary>
    /// Returns the number of steps <paramref name="a"/> can be left-shifted without overflow, or 0 if a == 0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short NormW32(int a)
    {
        if (a == 0) return 0;
        return (short)(CountLeadingZeros32(a < 0 ? (uint)~a : (uint)a) - 1);
    }

    /// <summary>
    /// Returns the number of steps <paramref name="a"/> can be left-shifted without overflow, or 0 if a == 0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short NormU32(uint a)
    {
        if (a == 0) return 0;
        return (short)CountLeadingZeros32(a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DivW32W16(int num, short den)
    {
        if (den != 0)
            return num / den;
        return 0x7FFFFFFF;
    }

    public static short GetScalingSquare(ReadOnlySpan<short> inVector, int times)
    {
        short nbits = GetSizeInBits((uint)times);
        short smax = -1;
        int i = 0;

#if NET9_0_OR_GREATER
        if (Vector128.IsHardwareAccelerated && inVector.Length >= Vector128<short>.Count)
        {
            ref short src = ref MemoryMarshal.GetReference(inVector);
            var vMax = Vector128<short>.Zero;
            for (; i <= inVector.Length - Vector128<short>.Count; i += Vector128<short>.Count)
            {
                var v = Vector128.LoadUnsafe(ref src, (nuint)i);
                vMax = Vector128.Max(vMax, Vector128.Abs(v));
            }
            // Horizontal max reduction
            vMax = Vector128.Max(vMax, Vector128.Shuffle(vMax, Vector128.Create((short)4, 5, 6, 7, 0, 1, 2, 3)));
            vMax = Vector128.Max(vMax, Vector128.Shuffle(vMax, Vector128.Create((short)2, 3, 0, 1, 4, 5, 6, 7)));
            vMax = Vector128.Max(vMax, Vector128.Shuffle(vMax, Vector128.Create((short)1, 0, 2, 3, 4, 5, 6, 7)));
            smax = vMax.GetElement(0);
        }
#endif

        for (; i < inVector.Length; i++)
        {
            short sabs = inVector[i] > 0 ? inVector[i] : (short)-inVector[i];
            if (sabs > smax) smax = sabs;
        }

        short t = NormW32(Mul(smax, smax));

        if (smax == 0)
            return 0;

        return t > nbits ? (short)0 : (short)(nbits - t);
    }

    public static int Energy(ReadOnlySpan<short> vector, out int scaleFactor)
    {
        int scaling = GetScalingSquare(vector, vector.Length);
        int en = 0;
        int i = 0;

#if NET9_0_OR_GREATER
        if (Vector128.IsHardwareAccelerated && vector.Length >= Vector128<short>.Count)
        {
            ref short src = ref MemoryMarshal.GetReference(vector);
            var acc = Vector128<int>.Zero;
            for (; i <= vector.Length - Vector128<short>.Count; i += Vector128<short>.Count)
            {
                var v = Vector128.LoadUnsafe(ref src, (nuint)i);
                var lo = Vector128.WidenLower(v);
                var hi = Vector128.WidenUpper(v);
                lo *= lo;
                hi *= hi;
                acc += Vector128.ShiftRightArithmetic(lo, scaling)
                     + Vector128.ShiftRightArithmetic(hi, scaling);
            }
            en = Vector128.Sum(acc);
        }
#endif

        for (; i < vector.Length; i++)
        {
            en += (vector[i] * vector[i]) >> scaling;
        }

        scaleFactor = scaling;
        return en;
    }
}