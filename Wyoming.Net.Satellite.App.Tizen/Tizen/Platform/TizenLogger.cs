using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using TizenLog = Tizen.Log;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal class TizenLogger : ILogger, IDisposable
{
    internal static readonly TizenLogger Singleton = new();
    const string TAG = "WYOMING";

    public static LogLevel Level = LogLevel.Information;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }

    public void Dispose()
    {
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= Level;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if(!IsEnabled(logLevel))
        {
            return;
        }

        var msg = formatter(state, exception);

        switch(logLevel)
        {
            case LogLevel.Trace:
            TizenLog.Verbose(TAG, msg);
            break;

            case LogLevel.Debug:
            TizenLog.Debug(TAG, msg);
            break;

            case LogLevel.Information:
            TizenLog.Info(TAG, msg);
            break;

            case LogLevel.Warning:
            TizenLog.Warn(TAG, msg);
            break;

            case LogLevel.Error:
            TizenLog.Error(TAG, string.Concat(msg, Environment.NewLine, exception?.ToString()));
            break;

            case LogLevel.Critical:
            TizenLog.Fatal(TAG, msg);
            break;
        }
        
        if(RemoteLogger.Singleton is not null && RemoteLogger.Singleton.Enabled)
        {
            RemoteLogger.Singleton.Log(msg, logLevel.ToString());
        }
    }
}

internal sealed class TizenLogger<TCategory> : TizenLogger, ILogger<TCategory>
{
}

internal sealed class TizenLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return TizenLogger.Singleton;
    }

    public void Dispose()
    {
    }
}
