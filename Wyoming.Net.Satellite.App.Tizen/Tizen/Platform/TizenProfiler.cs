using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tizen.System;
using Tizen.TV.System.Sensor;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal sealed class TizenProfiler : IDisposable
{
    private readonly Timer _timer;

    private readonly int _pid;
    private readonly string _app;

    private readonly int[] _pids;
    private readonly ProcessCpuUsage _processCpu;
    private readonly SystemCpuUsage _systemCpu;

    private readonly Stopwatch _clock;

    private readonly int _cpuCount;

    private uint _lastUserTicks;
    private uint _lastSystemTicks;
    private long _lastTimestamp;

    private bool _running;

    // Linux USER_HZ (clock ticks per second)
    private const double TicksPerSecond = 100.0;

    MotionSensor s = new MotionSensor(0);

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
            TizenLogger.Singleton.LogInformation("Motion supported: {s} - Count: {c}", Tizen.TV.System.Sensor.MotionSensor.IsSupported, MotionSensor.Count);

            
            // s.DataUpdated += (s, args) =>
            // {
            //     TizenLogger.Singleton.LogInformation("Motion: {m}", args.Motion);
            // };
            // s.Start();
           
            // foreach(var feat in TizenFeatures.All)
            // {
            //     bool got = false;
            //     object? value = null;

            //     if(feat.Type == "String")
            //     {
            //         got = Information.TryGetValue<string>(feat.Key, out var str);
            //         value = str;
            //     }
            //     else if(feat.Type == "bool")
            //     {
            //         got = Information.TryGetValue<bool>(feat.Key, out bool b);
            //         value = b;
            //     }
            //     else if(feat.Type == "int")
            //     {
            //         got = Information.TryGetValue<int>(feat.Key, out int i);
            //         value = i;
            //     }

            //     if(got)
            //     {
            //         TizenLogger.Singleton.LogInformation("Got feature: {f} - Value: {v}", feat.Key, value?.ToString());
            //     }
            //     else
            //     {
            //         TizenLogger.Singleton.LogInformation("Failed to get feature: {f}", feat.Key);
            //     }
            // }

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
            double userCpu = 0;
            double sysCpu = 0;

            if (elapsedSeconds > 0)
            {
                double divisor = elapsedSeconds * _cpuCount * TicksPerSecond / 100.0;
                cpuUsage = totalTicks / divisor;
                userCpu = userDelta / divisor;
                sysCpu = sysDelta / divisor;
            }

            _lastUserTicks = userTicks;
            _lastSystemTicks = systemTicks;
            _lastTimestamp = nowTicks;

            TizenLogger.Singleton.LogInformation(
                "[PROFILER][{App}] CPU:{Cpu:F2}% (user:{User:F2}% sys:{Sys:F2}%)",
                _app, cpuUsage, userCpu, sysCpu);
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

    public void Dispose()
    {
        _running = false;
        _timer?.Dispose();
    }
}