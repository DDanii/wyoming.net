using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Wyoming.Net.Core.Audio;
using Wyoming.Net.Satellite;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class WakeWordAudioDebugger
{
    private const int PreBufferFrames = 12;   // ~1 second of context before prediction
    private const int PostSilenceFrames = 6;  // ~0.5s of silence after predictions stop
    private const int MaxFiles = 100;
    private const int SampleRate = MicSettings.Rate; // 16000

    private readonly ILogger logger;
    private readonly string outputDir;

    // Rolling pre-buffer: circular buffer of the last PreBufferFrames frames
    private readonly float[][] preBuffer = new float[PreBufferFrames][];
    private int preBufferIndex;
    private int preBufferCount;

    // Recording state
    private bool isRecording;
    private int silenceCount;
    private float peakPrediction;
    private readonly List<float[]> recordingFrames = new();

    public WakeWordAudioDebugger(ILogger logger)
    {
        this.logger = logger;
        outputDir = TizenAssetReader.DataDir;
    }

    public void OnPrediction(float prediction, ReadOnlyMemory<float> audioFrame)
    {
        try
        {
            logger.LogDebug("OnPrediction: {Prediction:F3}, recording={IsRecording}", prediction, isRecording);

            var span = audioFrame.Span;

            // Copy frame data (exact size, no pooling — debug only)
            var frameCopy = span.ToArray();

            float threshold = SatelliteSettings.Wake.PredictionThreshold;

            if (prediction >= threshold)
            {
                if (!isRecording)
                {
                    // Start recording: flush pre-buffer into recording frames
                    isRecording = true;
                    silenceCount = 0;
                    peakPrediction = prediction;
                    recordingFrames.Clear();

                    // Add pre-buffer frames in order
                    int start = preBufferCount < PreBufferFrames ? 0 : preBufferIndex;
                    int count = Math.Min(preBufferCount, PreBufferFrames);
                    for (int i = 0; i < count; i++)
                    {
                        int idx = (start + i) % PreBufferFrames;
                        recordingFrames.Add(preBuffer[idx]);
                        preBuffer[idx] = null!; // ownership transferred
                    }
                    preBufferCount = 0;
                    preBufferIndex = 0;
                }

                silenceCount = 0;
                peakPrediction = Math.Max(peakPrediction, prediction);
                recordingFrames.Add(frameCopy);
            }
            else if (isRecording)
            {
                silenceCount++;
                recordingFrames.Add(frameCopy);

                if (silenceCount >= PostSilenceFrames)
                {
                    FlushRecording();
                }
            }
            else
            {
                // Not recording, not above threshold: update pre-buffer
                preBuffer[preBufferIndex] = frameCopy;
                preBufferIndex = (preBufferIndex + 1) % PreBufferFrames;
                if (preBufferCount < PreBufferFrames) preBufferCount++;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in OnPrediction");
        }
    }

    private void FlushRecording()
    {
        try
        {
            int totalSamples = 0;
            foreach (var frame in recordingFrames)
            {
                totalSamples += frame.Length;
            }

            int pcmByteCount = totalSamples * 2; // 16-bit = 2 bytes per sample
            byte[] pcmData = new byte[pcmByteCount];
            int offset = 0;

            foreach (var frame in recordingFrames)
            {
                AudioOp.FloatToPcm16(frame.AsSpan(), pcmData.AsSpan(offset));
                offset += frame.Length * 2;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"ww_debug_{timestamp}_{peakPrediction:F2}.wav";
            string filePath = Path.Combine(outputDir, fileName);

            WriteWav(filePath, pcmData, SampleRate, 1, 16);

            logger.LogInformation("Debug audio saved: {FilePath} ({Frames} frames, peak {Peak:F3})",
                filePath, recordingFrames.Count, peakPrediction);

            EnforceMaxFiles();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to flush debug audio recording");
        }
        finally
        {
            recordingFrames.Clear();
            isRecording = false;
            silenceCount = 0;
            peakPrediction = 0;
        }
    }

    private static readonly byte[] RiffHeader = System.Text.Encoding.ASCII.GetBytes("RIFF");
    private static readonly byte[] WaveHeader = System.Text.Encoding.ASCII.GetBytes("WAVE");
    private static readonly byte[] FmtHeader = System.Text.Encoding.ASCII.GetBytes("fmt ");
    private static readonly byte[] DataHeader = System.Text.Encoding.ASCII.GetBytes("data");

    private static void WriteWav(string path, byte[] pcmData, int sampleRate, int channels, int bitsPerSample)
    {
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        short blockAlign = (short)(channels * bitsPerSample / 8);

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        // RIFF header
        bw.Write(RiffHeader);
        bw.Write(36 + pcmData.Length); // ChunkSize
        bw.Write(WaveHeader);

        // fmt sub-chunk
        bw.Write(FmtHeader);
        bw.Write(16);                          // SubChunk1Size (PCM)
        bw.Write((short)1);                    // AudioFormat (PCM)
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write((short)bitsPerSample);

        // data sub-chunk
        bw.Write(DataHeader);
        bw.Write(pcmData.Length);
        bw.Write(pcmData);
    }

    private void EnforceMaxFiles()
    {
        try
        {
            var files = Directory.GetFiles(outputDir, "ww_debug_*.wav")
                .OrderBy(File.GetCreationTime)
                .ToArray();

            int toDelete = files.Length - MaxFiles;
            for (int i = 0; i < toDelete; i++)
            {
                File.Delete(files[i]);
                logger.LogDebug("Deleted old debug audio: {File}", files[i]);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enforce max debug audio files");
        }
    }
}
