using System.Runtime.CompilerServices;
// ReSharper disable InconsistentNaming

namespace Wyoming.Net.Core.WebRtc.Vad.Internal;

internal static class VadCore
{
      private static ReadOnlySpan<short> SpectrumWeight => new short[] { 6, 8, 10, 12, 14, 16 };
    private const short NoiseUpdateConst = 655;     // Q15
    private const short SpeechUpdateConst = 6554;   // Q15
    private const short BackEta = 154;              // Q8

    private static ReadOnlySpan<short> MinimumDifference => new short[] { 544, 544, 576, 576, 576, 576 };
    private static ReadOnlySpan<short> MaximumSpeech => new short[] { 11392, 11392, 11520, 11520, 11520, 11520 };
    private static ReadOnlySpan<short> MinimumMean => new short[] { 640, 768 };
    private static ReadOnlySpan<short> MaximumNoise => new short[] { 9216, 9088, 8960, 8832, 8704, 8576 };

    private static ReadOnlySpan<short> NoiseDataWeights => new short[] { 34, 62, 72, 66, 53, 25, 94, 66, 56, 62, 75, 103 };
    private static ReadOnlySpan<short> SpeechDataWeights => new short[] { 48, 82, 45, 87, 50, 47, 80, 46, 83, 41, 78, 81 };
    private static ReadOnlySpan<short> NoiseDataMeans => new short[] { 6738, 4892, 7065, 6715, 6771, 3369, 7646, 3863, 7820, 7266, 5020, 4362 };
    private static ReadOnlySpan<short> SpeechDataMeans => new short[] { 8306, 10085, 10078, 11823, 11843, 6309, 9473, 9571, 10879, 7581, 8180, 7483 };
    private static ReadOnlySpan<short> NoiseDataStds => new short[] { 378, 1064, 493, 582, 688, 593, 474, 697, 475, 688, 421, 455 };
    private static ReadOnlySpan<short> SpeechDataStds => new short[] { 555, 505, 567, 524, 585, 1231, 509, 828, 492, 1540, 1079, 850 };

    private const short MaxSpeechFrames = 6;
    private const short MinStd = 384;
    private const int DefaultMode = 0;
    private const int InitCheck = 42;

    // Mode thresholds: [10ms, 20ms, 30ms]
    private static ReadOnlySpan<short> OverHangMax1Q => new short[] { 8, 4, 3 };
    private static ReadOnlySpan<short> OverHangMax2Q => new short[] { 14, 7, 5 };
    private static ReadOnlySpan<short> LocalThresholdQ => new short[] { 24, 21, 24 };
    private static ReadOnlySpan<short> GlobalThresholdQ => new short[] { 57, 48, 57 };

    private static ReadOnlySpan<short> OverHangMax1LBR => new short[] { 8, 4, 3 };
    private static ReadOnlySpan<short> OverHangMax2LBR => new short[] { 14, 7, 5 };
    private static ReadOnlySpan<short> LocalThresholdLBR => new short[] { 37, 32, 37 };
    private static ReadOnlySpan<short> GlobalThresholdLBR => new short[] { 100, 80, 100 };

    private static ReadOnlySpan<short> OverHangMax1AGG => new short[] { 6, 3, 2 };
    private static ReadOnlySpan<short> OverHangMax2AGG => new short[] { 9, 5, 3 };
    private static ReadOnlySpan<short> LocalThresholdAGG => new short[] { 82, 78, 82 };
    private static ReadOnlySpan<short> GlobalThresholdAGG => new short[] { 285, 260, 285 };

    private static ReadOnlySpan<short> OverHangMax1VAG => new short[] { 6, 3, 2 };
    private static ReadOnlySpan<short> OverHangMax2VAG => new short[] { 9, 5, 3 };
    private static ReadOnlySpan<short> LocalThresholdVAG => new short[] { 94, 94, 94 };
    private static ReadOnlySpan<short> GlobalThresholdVAG => new short[] { 1100, 1050, 1100 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WeightedAverage(Span<short> data, short offset, ReadOnlySpan<short> weights)
    {
        int weightedAverage = 0;
        for (int k = 0; k < VadInst.NumGaussians; k++)
        {
            data[k * VadInst.NumChannels] += offset;
            weightedAverage += data[k * VadInst.NumChannels] * weights[k * VadInst.NumChannels];
        }
        return weightedAverage;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int OverflowingMulS16ByS32ToS32(short a, int b)
    {
        return unchecked(a * b);
    }

    [SkipLocalsInit]
    private static short GmmProbability(VadInst self, Span<short> features,
        short totalPower, int frameLength)
    {
        short vadflag = 0;
        int sumLogLikelihoodRatios = 0;

        Span<short> deltaN = stackalloc short[VadInst.TableSize];
        Span<short> deltaS = stackalloc short[VadInst.TableSize];
        Span<short> ngprvec = stackalloc short[VadInst.TableSize];
        Span<short> sgprvec = stackalloc short[VadInst.TableSize];
        ngprvec.Clear();
        sgprvec.Clear();

        // Set thresholds based on frame length (80, 160, or 240 samples)
        int frameIdx = frameLength == 80 ? 0 : (frameLength == 160 ? 1 : 2);
        short overhead1 = self.OverHangMax1[frameIdx];
        short overhead2 = self.OverHangMax2[frameIdx];
        short individualTest = self.Individual[frameIdx];
        short totalTest = self.Total[frameIdx];

        ReadOnlySpan<short> specWeight = SpectrumWeight;
        ReadOnlySpan<short> noiseWeights = NoiseDataWeights;
        ReadOnlySpan<short> speechWeights = SpeechDataWeights;
        ReadOnlySpan<short> minDiff = MinimumDifference;
        ReadOnlySpan<short> maxSpeech = MaximumSpeech;
        ReadOnlySpan<short> minMean = MinimumMean;
        ReadOnlySpan<short> maxNoise = MaximumNoise;

        Span<int> noiseProbability = stackalloc int[VadInst.NumGaussians];
        Span<int> speechProbability = stackalloc int[VadInst.NumGaussians];

        if (totalPower > VadInst.MinEnergy)
        {
            for (int channel = 0; channel < VadInst.NumChannels; channel++)
            {
                int h0Test = 0;
                int h1Test = 0;

                for (int k = 0; k < VadInst.NumGaussians; k++)
                {
                    int gaussian = channel + k * VadInst.NumChannels;

                    int tmp1S32 = VadGmm.GaussianProbability(features[channel],
                        self.NoiseMeans[gaussian], self.NoiseStds[gaussian],
                        out deltaN[gaussian]);
                    noiseProbability[k] = noiseWeights[gaussian] * tmp1S32;
                    h0Test += noiseProbability[k];

                    tmp1S32 = VadGmm.GaussianProbability(features[channel],
                        self.SpeechMeans[gaussian], self.SpeechStds[gaussian],
                        out deltaS[gaussian]);
                    speechProbability[k] = speechWeights[gaussian] * tmp1S32;
                    h1Test += speechProbability[k];
                }

                short shiftsH0 = SignalProcessing.NormW32(h0Test);
                short shiftsH1 = SignalProcessing.NormW32(h1Test);
                if (h0Test == 0) shiftsH0 = 31;
                if (h1Test == 0) shiftsH1 = 31;
                short logLikelihoodRatio = (short)(shiftsH0 - shiftsH1);

                sumLogLikelihoodRatios += logLikelihoodRatio * specWeight[channel];

                if ((logLikelihoodRatio * 4) > individualTest)
                    vadflag = 1;

                // Noise conditional probabilities
                short h0 = (short)(h0Test >> 12);
                if (h0 > 0)
                {
                    int tmp1S32 = (noiseProbability[0] & unchecked((int)0xFFFFF000)) << 2;
                    ngprvec[channel] = (short)SignalProcessing.DivW32W16(tmp1S32, h0);
                    ngprvec[channel + VadInst.NumChannels] = (short)(16384 - ngprvec[channel]);
                }
                else
                {
                    ngprvec[channel] = 16384;
                }

                // Speech conditional probabilities
                short h1 = (short)(h1Test >> 12);
                if (h1 > 0)
                {
                    int tmp1S32 = (speechProbability[0] & unchecked((int)0xFFFFF000)) << 2;
                    sgprvec[channel] = (short)SignalProcessing.DivW32W16(tmp1S32, h1);
                    sgprvec[channel + VadInst.NumChannels] = (short)(16384 - sgprvec[channel]);
                }
            }

            vadflag |= (short)(sumLogLikelihoodRatios >= totalTest ? 1 : 0);

            // Update model parameters
            short maxspe = 12800;
            for (int channel = 0; channel < VadInst.NumChannels; channel++)
            {
                short featureMinimum = VadSp.FindMinimum(self, features[channel], channel);

                int noiseGlobalMean = WeightedAverage(
                    self.NoiseMeans.AsSpan(channel), 0, noiseWeights[channel..]);
                short tmp1S16 = (short)(noiseGlobalMean >> 6);

                for (int k = 0; k < VadInst.NumGaussians; k++)
                {
                    int gaussian = channel + k * VadInst.NumChannels;
                    short nmk = self.NoiseMeans[gaussian];
                    short smk = self.SpeechMeans[gaussian];
                    short nsk = self.NoiseStds[gaussian];
                    short ssk = self.SpeechStds[gaussian];

                    // Update noise mean
                    short nmk2 = nmk;
                    if (vadflag == 0)
                    {
                        short delt = (short)((ngprvec[gaussian] * deltaN[gaussian]) >> 11);
                        nmk2 = (short)(nmk + (short)((delt * NoiseUpdateConst) >> 22));
                    }

                    // Long term correction
                    short ndelt = (short)((featureMinimum << 4) - tmp1S16);
                    short nmk3 = (short)(nmk2 + (short)((ndelt * BackEta) >> 9));

                    // Clamp noise mean
                    short tmpS16 = (short)((k + 5) << 7);
                    if (nmk3 < tmpS16) nmk3 = tmpS16;
                    tmpS16 = (short)((72 + k - channel) << 7);
                    if (nmk3 > tmpS16) nmk3 = tmpS16;
                    self.NoiseMeans[gaussian] = nmk3;

                    if (vadflag != 0)
                    {
                        // Update speech mean
                        short delt = (short)((sgprvec[gaussian] * deltaS[gaussian]) >> 11);
                        tmpS16 = (short)((delt * SpeechUpdateConst) >> 21);
                        short smk2 = (short)(smk + ((tmpS16 + 1) >> 1));

                        short maxmu = (short)(maxspe + 640);
                        if (smk2 < minMean[k]) smk2 = minMean[k];
                        if (smk2 > maxmu) smk2 = maxmu;
                        self.SpeechMeans[gaussian] = smk2;

                        tmpS16 = (short)((smk + 4) >> 3);
                        tmpS16 = (short)(features[channel] - tmpS16);
                        int tmp1S32 = (deltaS[gaussian] * tmpS16) >> 3;
                        int tmp2S32 = tmp1S32 - 4096;
                        tmpS16 = (short)(sgprvec[gaussian] >> 2);
                        tmp1S32 = tmpS16 * tmp2S32;
                        tmp2S32 = tmp1S32 >> 4;

                        if (tmp2S32 > 0)
                            tmpS16 = (short)SignalProcessing.DivW32W16(tmp2S32, (short)(ssk * 10));
                        else
                        {
                            tmpS16 = (short)SignalProcessing.DivW32W16(-tmp2S32, (short)(ssk * 10));
                            tmpS16 = (short)-tmpS16;
                        }

                        tmpS16 += 128;
                        ssk += (short)(tmpS16 >> 8);
                        if (ssk < MinStd) ssk = MinStd;
                        self.SpeechStds[gaussian] = ssk;
                    }
                    else
                    {
                        // Update noise variance
                        tmpS16 = (short)(features[channel] - (nmk >> 3));
                        int tmp1S32 = (deltaN[gaussian] * tmpS16) >> 3;
                        tmp1S32 -= 4096;

                        tmpS16 = (short)((ngprvec[gaussian] + 2) >> 2);
                        int tmp2S32 = OverflowingMulS16ByS32ToS32(tmpS16, tmp1S32);
                        tmp1S32 = tmp2S32 >> 14;

                        if (tmp1S32 > 0)
                            tmpS16 = (short)SignalProcessing.DivW32W16(tmp1S32, nsk);
                        else
                        {
                            tmpS16 = (short)SignalProcessing.DivW32W16(-tmp1S32, nsk);
                            tmpS16 = (short)-tmpS16;
                        }

                        tmpS16 += 32;
                        nsk += (short)(tmpS16 >> 6);
                        if (nsk < MinStd) nsk = MinStd;
                        self.NoiseStds[gaussian] = nsk;
                    }
                }

                // Separate models if too close
                int noiseGlobalMean2 = WeightedAverage(
                    self.NoiseMeans.AsSpan(channel), 0, noiseWeights[channel..]);
                int speechGlobalMean = WeightedAverage(
                    self.SpeechMeans.AsSpan(channel), 0, speechWeights[channel..]);

                short diff = (short)((short)(speechGlobalMean >> 9) - (short)(noiseGlobalMean2 >> 9));
                if (diff < minDiff[channel])
                {
                    short tmpS = (short)(minDiff[channel] - diff);
                    short tmp1 = (short)((13 * tmpS) >> 2);
                    short tmp2 = (short)((3 * tmpS) >> 2);

                    speechGlobalMean = WeightedAverage(
                        self.SpeechMeans.AsSpan(channel), tmp1, speechWeights[channel..]);
                    noiseGlobalMean2 = WeightedAverage(
                        self.NoiseMeans.AsSpan(channel), (short)-tmp2, noiseWeights[channel..]);
                }

                // Clamp speech & noise means
                maxspe = maxSpeech[channel];
                short tmp2S16 = (short)(speechGlobalMean >> 7);
                if (tmp2S16 > maxspe)
                {
                    tmp2S16 -= maxspe;
                    for (int k = 0; k < VadInst.NumGaussians; k++)
                        self.SpeechMeans[channel + k * VadInst.NumChannels] -= tmp2S16;
                }

                tmp2S16 = (short)(noiseGlobalMean2 >> 7);
                if (tmp2S16 > maxNoise[channel])
                {
                    tmp2S16 -= maxNoise[channel];
                    for (int k = 0; k < VadInst.NumGaussians; k++)
                        self.NoiseMeans[channel + k * VadInst.NumChannels] -= tmp2S16;
                }
            }

            self.FrameCounter++;
        }

        // Transition hysteresis smoothing
        if (vadflag == 0)
        {
            if (self.OverHang > 0)
            {
                vadflag = (short)(2 + self.OverHang);
                self.OverHang--;
            }
            self.NumOfSpeech = 0;
        }
        else
        {
            self.NumOfSpeech++;
            if (self.NumOfSpeech > MaxSpeechFrames)
            {
                self.NumOfSpeech = MaxSpeechFrames;
                self.OverHang = overhead2;
            }
            else
            {
                self.OverHang = overhead1;
            }
        }

        return vadflag;
    }

    public static int InitCore(VadInst self)
    {
        self.Vad = 1;
        self.FrameCounter = 0;
        self.OverHang = 0;
        self.NumOfSpeech = 0;

        Array.Clear(self.DownsamplingFilterStates);
        self.State48To8.Reset();

        ReadOnlySpan<short> noiseMeans = NoiseDataMeans;
        ReadOnlySpan<short> speechMeans = SpeechDataMeans;
        ReadOnlySpan<short> noiseStds = NoiseDataStds;
        ReadOnlySpan<short> speechStds = SpeechDataStds;

        for (int i = 0; i < VadInst.TableSize; i++)
        {
            self.NoiseMeans[i] = noiseMeans[i];
            self.SpeechMeans[i] = speechMeans[i];
            self.NoiseStds[i] = noiseStds[i];
            self.SpeechStds[i] = speechStds[i];
        }

        for (int i = 0; i < 16 * VadInst.NumChannels; i++)
        {
            self.LowValueVector[i] = 10000;
            self.IndexVector[i] = 0;
        }

        Array.Clear(self.UpperState);
        Array.Clear(self.LowerState);
        Array.Clear(self.HpFilterState);

        for (int i = 0; i < VadInst.NumChannels; i++)
            self.MeanValue[i] = 1600;

        if (SetModeCore(self, DefaultMode) != 0)
            return -1;

        self.InitFlag = InitCheck;
        return 0;
    }

    public static int SetModeCore(VadInst self, int mode)
    {
        ReadOnlySpan<short> oh1, oh2, local, global;

        switch (mode)
        {
            case 0:
                oh1 = OverHangMax1Q; oh2 = OverHangMax2Q;
                local = LocalThresholdQ; global = GlobalThresholdQ;
                break;
            case 1:
                oh1 = OverHangMax1LBR; oh2 = OverHangMax2LBR;
                local = LocalThresholdLBR; global = GlobalThresholdLBR;
                break;
            case 2:
                oh1 = OverHangMax1AGG; oh2 = OverHangMax2AGG;
                local = LocalThresholdAGG; global = GlobalThresholdAGG;
                break;
            case 3:
                oh1 = OverHangMax1VAG; oh2 = OverHangMax2VAG;
                local = LocalThresholdVAG; global = GlobalThresholdVAG;
                break;
            default:
                return -1;
        }

        oh1.CopyTo(self.OverHangMax1);
        oh2.CopyTo(self.OverHangMax2);
        local.CopyTo(self.Individual);
        global.CopyTo(self.Total);

        return 0;
    }

    [SkipLocalsInit]
    public static int CalcVad48khz(VadInst inst, ReadOnlySpan<short> speechFrame, int frameLength)
    {
        Span<short> speechNb = stackalloc short[240];
        Span<int> tmpMem = stackalloc int[480 + 256];
        tmpMem.Clear();

        const int frameLen10ms48khz = 480;
        const int frameLen10ms8khz = 80;
        int num10msFrames = frameLength / frameLen10ms48khz;

        for (int i = 0; i < num10msFrames; i++)
        {
            // The original C code always passes the same input pointer (no advance).
            Resampler.Resample48khzTo8khz(
                speechFrame[..frameLen10ms48khz],
                speechNb.Slice(i * frameLen10ms8khz, frameLen10ms8khz),
                inst.State48To8,
                tmpMem);
        }

        return CalcVad8khz(inst, speechNb, frameLength / 6);
    }

    [SkipLocalsInit]
    public static int CalcVad32khz(VadInst inst, ReadOnlySpan<short> speechFrame, int frameLength)
    {
        Span<short> speechWB = stackalloc short[480];
        Span<short> speechNB = stackalloc short[240];

        VadSp.Downsampling(speechFrame[..frameLength], speechWB,
            inst.DownsamplingFilterStates.AsSpan(2, 2));
        int len = frameLength / 2;

        VadSp.Downsampling(speechWB[..len], speechNB,
            inst.DownsamplingFilterStates.AsSpan(0, 2));
        len /= 2;

        return CalcVad8khz(inst, speechNB, len);
    }

    [SkipLocalsInit]
    public static int CalcVad16khz(VadInst inst, ReadOnlySpan<short> speechFrame, int frameLength)
    {
        Span<short> speechNB = stackalloc short[240];

        VadSp.Downsampling(speechFrame[..frameLength], speechNB,
            inst.DownsamplingFilterStates.AsSpan(0, 2));

        return CalcVad8khz(inst, speechNB, frameLength / 2);
    }

    public static int CalcVad8khz(VadInst inst, ReadOnlySpan<short> speechFrame, int frameLength)
    {
        inst.TotalPower = VadFilterbank.CalculateFeatures(inst, speechFrame, frameLength,
            inst.FeatureVector);

        inst.Vad = GmmProbability(inst, inst.FeatureVector, inst.TotalPower, frameLength);

        return inst.Vad;
    }
}