using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class TizenProfiler : IDisposable
{
    private readonly Timer _timer;

    private readonly int _pid;
    private readonly string _app;

    private readonly int[] _pids;
    private readonly Tizen.System.ProcessCpuUsage _processCpu;
    private readonly Tizen.System.SystemCpuUsage _systemCpu;

    private readonly Stopwatch _clock;

    private readonly int _cpuCount;

    private uint _lastUserTicks;
    private uint _lastSystemTicks;
    private long _lastTimestamp;

    private bool _running;

    // Linux USER_HZ (clock ticks per second)
    private const double TicksPerSecond = 100.0;

    public TizenProfiler(string app, int intervalMs = 5000)
    {
        try
        {
            _pid = new Tizen.Applications.ApplicationRunningContext(Constants.ServiceAppId).ProcessId;

            _pids = new[] { _pid };

            _processCpu = new Tizen.System.ProcessCpuUsage(_pids);
            _systemCpu = new Tizen.System.SystemCpuUsage();

            _cpuCount = _systemCpu.ProcessorCount;
            TizenLogger.Singleton.LogInformation("CPU COUNT: " + _cpuCount);
            TizenLogger.Singleton.LogInformation("Adv supported: {s}", System.Runtime.Intrinsics.Arm.AdvSimd.IsSupported);
            TizenLogger.Singleton.LogInformation("Vector128 supported: {s}", Vector.IsHardwareAccelerated);

            _clock = Stopwatch.StartNew();

            // initial sample
            _processCpu.Update(_pids);

            _lastUserTicks = _processCpu.GetUTime(_pid);
            _lastSystemTicks = _processCpu.GetSTime(_pid);
            _lastTimestamp = _clock.ElapsedTicks;

            _app = app;
            _running = true;

            _timer = new Timer(Profile, null, intervalMs, intervalMs);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogInformation("profiler error: " + ex);
        }
    }

    private void Profile(object? state)
    {
        if (!_running)
            return;

        try
        {
            _processCpu.Update(_pids);

            uint userTicks = _processCpu.GetUTime(_pid);
            uint systemTicks = _processCpu.GetSTime(_pid);

            long nowTicks = _clock.ElapsedTicks;

            uint userDelta = Delta(userTicks, _lastUserTicks);
            uint sysDelta = Delta(systemTicks, _lastSystemTicks);

            uint totalTicks = userDelta + sysDelta;

            double elapsedSeconds =
                (nowTicks - _lastTimestamp) / (double)Stopwatch.Frequency;

            double cpuUsage = 0;

            if (elapsedSeconds > 0)
            {
                double cpuSeconds = totalTicks / TicksPerSecond;
                cpuUsage = cpuSeconds / (elapsedSeconds * _cpuCount) * 100.0;
            }

            _lastUserTicks = userTicks;
            _lastSystemTicks = systemTicks;
            _lastTimestamp = nowTicks;

            // Memory
            long managedMemory = GC.GetTotalMemory(false);

            // GC stats
            int gen0 = GC.CollectionCount(0);
            int gen1 = GC.CollectionCount(1);
            int gen2 = GC.CollectionCount(2);

            Log(managedMemory, cpuUsage, gen0, gen1, gen2, _app);
        }
        catch
        {
            // never crash profiler
        }
    }

    private static uint Delta(uint current, uint previous)
    {
        if (current >= previous)
            return current - previous;

        return (uint.MaxValue - previous) + current;
    }

    private static void Log(long managed, double cpu, int gen0, int gen1, int gen2, string app)
    {
        double managedMb = managed / 1024d / 1024d;

        TizenLogger.Singleton.LogInformation(
            "[PROFILER][{App}] CPU:{Cpu:F2}% Managed:{ManagedMb:F2}MB G0:{Gen0} G1:{Gen1} G2:{Gen2}",
            app, cpu, managedMb, gen0, gen1, gen2);
    }

    public void Dispose()
    {
        _running = false;
        _timer?.Dispose();
    }
}