using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tizen.Applications;
using Tizen.Applications.Messages;
using Tizen.System;
using Tizen.TV.System.Sensor;
using Wyoming.Net.Core;
using Wyoming.Net.Satellite.App.Tz.Platform;
using Wyoming.Net.Satellite.App.Tz.Platform.Interop;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz;

public sealed class BackgroundApp : ServiceApplication
{
    private MessagePort _localPort = new MessagePort(Constants.ServicePortName, false);

    private TizenAudioFocusManager? _audioFocusManager;

    private TizenSpeakerProvider? _speakerProvider;

    private WakeWordSatellite? _satellite;

    private ForegroundAppMonitor? _foregroundAppMonitor;

    private List<string> _unactiveApps = new();

    private bool _stoppedByMonitor;

    private MotionSensor? _motionSensor;
    private Timer? _noMotionTimer;
    private bool _stoppedByMotion;
    private int _noMotionTimeoutSeconds;

    private DebugFileServer? _debugFileServer;

    private SatelliteSettingsViewModel _settings = null!;

    protected override void OnCreate()
    {
        try
        {
            base.OnCreate();

            _settings = SatelliteSettingsViewModel.Load();
            RemoteLogger.InitSingleton(
                _settings.ControlPanel.RemoteLogIp,
                _settings.ControlPanel.RemoteLogPort);

            TizenLogger.Level = LogLevel.Debug;

            ManagePowerLock(true);

            _localPort.MessageReceived += OnMessageReceived;
            _localPort.Listen();

            
            _unactiveApps = _settings.StateConfiguration.UnactiveApps;
            _foregroundAppMonitor = new ForegroundAppMonitor(TizenLogger.Singleton);
            _foregroundAppMonitor.ForegroundAppChanged += OnForegroundAppChanged;
            _foregroundAppMonitor.Start(_settings.StateConfiguration.WatcherIntervalSeconds * 1000);

            ConfigureMotionSensor();
            ConfigureDebugFileServer();

            NativeDisplay.TurnOnScreen();
        }
        catch (Exception e)
        {
            TizenLogger.Singleton.LogError(e, "Error in OnCreate");
        }
    }


    private static void ManagePowerLock(bool acquiring)
    {
        try
        {
            if (acquiring)
            {
                Power.RequestLock(PowerLock.Cpu, 0);
            }
            else
            {
                Power.ReleaseLock(PowerLock.Cpu);
            }
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Failed to acquire power lock");
        }
    }

    protected override async void OnTerminate()
    {
        try
        {
            _debugFileServer?.Dispose();
            _debugFileServer = null;
            DisposeMotionSensor();
            _foregroundAppMonitor?.Dispose();
            ManagePowerLock(false);
            await StopSatellite();
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in OnTerminate");
        }
        finally
        {
            base.OnTerminate();
        }
    }

    protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
    {
        try
        {
            ReceivedAppControl receivedAppControl = e.ReceivedAppControl;

            if (receivedAppControl.IsReplyRequest)
            {
                AppControl replyRequest = new();
                receivedAppControl.ReplyToLaunchRequest(replyRequest, AppControlReplyResult.Succeeded);
            }

            base.OnAppControlReceived(e);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in OnAppControlReceived");
        }
    }

    private async void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
    {
        try
        {
            var command = args.Message.GetItem<string>(Constants.Commands.CommandKey);

            switch (command)
            {
                case Constants.Commands.StartCommand:
                    _stoppedByMonitor = false;
                    await StartSatellite();
                    break;
                case Constants.Commands.StopCommand:
                    await StopSatellite();
                    break;
                case Constants.Commands.GetStatusCommand:
                    NotifyUiState();
                    break;
                case Constants.Commands.PingCommand:
                    ReplyToPing();
                    break;
                case Constants.Commands.ReloadSettingsCommand:
                    ReloadSettings();
                    break;
            }
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in OnMessageReceived");
        }
    }

    private async void OnForegroundAppChanged(string appId)
    {
        try
        {
            bool isUnactive = _unactiveApps.Contains(appId);

            if (isUnactive && _satellite != null && _satellite.IsRunning)
            {
                TizenLogger.Singleton.LogInformation("Stopping satellite: inactive app '{AppId}' is in foreground", appId);
                _stoppedByMonitor = true;

                await _satellite.MuteAsync();
            }
            else if (!isUnactive && _stoppedByMonitor)
            {
                TizenLogger.Singleton.LogInformation("Starting satellite: inactive app no longer in foreground (now '{AppId}')", appId);
                _stoppedByMonitor = false;

                await StartSatellite();
                await _satellite!.UnMuteAsync();
            }
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in OnForegroundAppChanged");
        }
    }

    private void ReloadSettings()
    {
        try
        {
            _settings = SatelliteSettingsViewModel.Load();
            _unactiveApps = _settings.StateConfiguration.UnactiveApps;
            ConfigureMotionSensor();
            ConfigureDebugFileServer();
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in ReloadSettings");
        }
    }

    private void ConfigureMotionSensor()
    {
        try
        {
            var enabled = _settings.PowerStateSettings.MotionSensorEnabled;
            _noMotionTimeoutSeconds = _settings.PowerStateSettings.NoMotionTimeoutSeconds;

            if (enabled && _motionSensor == null)
            {
                _motionSensor = new MotionSensor(0);
                _motionSensor.DataUpdated += OnMotionSensorDataUpdated;
                _motionSensor.Start();
            }
            else if (!enabled && _motionSensor != null)
            {
                DisposeMotionSensor();
            }
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in ConfigureMotionSensor");
        }
    }

    private void ConfigureDebugFileServer()
    {
        try
        {
            var enabled = _settings.ControlPanel.DebugFileServerEnabled;

            if (enabled && _debugFileServer == null)
            {
                _debugFileServer = new DebugFileServer(TizenLogger.Singleton);
                _debugFileServer.Start();
            }
            else if (!enabled && _debugFileServer != null)
            {
                _debugFileServer.Dispose();
                _debugFileServer = null;
            }
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in ConfigureDebugFileServer");
        }
    }

    private void DisposeMotionSensor()
    {
        if (_motionSensor != null)
        {
            _motionSensor.DataUpdated -= OnMotionSensorDataUpdated;
            _motionSensor.Stop();
            _motionSensor.Dispose();
            _motionSensor = null;
        }

        _noMotionTimer?.Dispose();
        _noMotionTimer = null;
    }

    private async void OnMotionSensorDataUpdated(object? sender, MotionSensorDataUpdatedEventArgs e)
    {
        try
        {
            bool motionDetected = e.Motion > 0;

            if (motionDetected)
            {
                await OnMotionDetected();
            }
            else
            {
                OnNoMotion();
            }
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in OnMotionSensorDataUpdated");
        }
    }

    private void OnNoMotion()
    {
        if (_noMotionTimer == null && _satellite is not null && _satellite.IsRunning && !_satellite.MicMuted && !_stoppedByMonitor)
        {
            _noMotionTimer = new Timer(async _ =>
            {
                TizenLogger.Singleton.LogInformation("Stopping satellite: no motion detected for {Seconds}s", _noMotionTimeoutSeconds);

                if (_settings.PowerStateSettings.TurnOffScreen)
                {
                    NativeDisplay.TurnOffScreen();
                }

                _stoppedByMotion = true;

                await _satellite.MuteAsync();

                _foregroundAppMonitor?.Stop();
                _noMotionTimer?.Dispose();
                _noMotionTimer = null;

            }, null, _noMotionTimeoutSeconds * 1000, Timeout.Infinite);
        }
    }

    private async Task OnMotionDetected()
    {
        _noMotionTimer?.Dispose();
        _noMotionTimer = null;

        if (_stoppedByMotion)
        {
            TizenLogger.Singleton.LogInformation("Starting satellite: motion detected");

            if (_settings.PowerStateSettings.TurnOffScreen)
            {
                NativeDisplay.TurnOnScreen();
            }
            _stoppedByMotion = false;
            _foregroundAppMonitor?.Start(_settings.StateConfiguration.WatcherIntervalSeconds * 1000);

            await StartSatellite();
            await _satellite!.UnMuteAsync();
        }
    }

    private async Task StartSatellite()
    {
        try
        {
            if (_satellite != null && _satellite.IsRunning)
            {
                return;
            }

            ReloadSettings();
            var settingsViewModel = _settings;
            settingsViewModel.UpdateSatelliteSettings();
            var wakeModels = await settingsViewModel.WakeSettings.GetModelsAsync();
            var loggerFactory = new TizenLoggerFactory();
            var logger = loggerFactory.CreateLogger(string.Empty);

            _audioFocusManager = new TizenAudioFocusManager(logger);
            _speakerProvider = new TizenSpeakerProvider(_audioFocusManager);

            _satellite = new WakeWordSatellite(wakeModels, loggerFactory, new TizenMicProvider(logger), _speakerProvider);
            _satellite.StateChanged += () => NotifyUiState();
            _satellite.WakeWordDetected += HandleWakeWordDetected;
            _satellite.SatelliteError += NotifyError;

            if (settingsViewModel.ControlPanel.DebugAudioEnabled)
            {
                TizenLogger.Singleton.LogInformation("DebugAudio is Enabled!");
                
                var audioDebugger = new WakeWordAudioDebugger(TizenLogger.Singleton);
                _satellite.DebugPredictionCallback = audioDebugger.OnPrediction;
            }

            TizenServer.CreateSingleton(_satellite, settingsViewModel, loggerFactory);

            await TizenServer.Singleton!.StartAsync();

// #if DEBUG
//             LaunchProfiler();
// #endif

            NotifyUiState(true);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in StartSatellite");
        }
    }

    private async Task HandleWakeWordDetected()
    {
        try
        {
            Asserts.IsNotNull(_speakerProvider);

            var wav = await TizenAssetReader.ReadAssetAsync("ww_detected3.wav");
            var wavInfo = WavHelper.ReadWavInfo(wav);
            await _speakerProvider!.StartAsync(wavInfo.SampleRate, wavInfo.BytesPerSample, wavInfo.Channels);
            await _speakerProvider.PlayAsync(wav, null);
            await _speakerProvider.StopAsync();

            SendWakeWordDetected();
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in HandleWakeWordDetected");
        }
    }

    private void NotifyUiState(bool isConnecting = false)
    {
        try
        {
            if (_satellite == null || ApplicationHelper.CheckUiState() != ApplicationRunningContext.AppState.Foreground)
            {
                return;
            }

            Bundle msg = new();
            msg.AddItem(Constants.Events.EventKey, Constants.Events.StateChangedEvent);
            msg.AddItem("isRunning", _satellite.IsRunning.ToString());
            msg.AddItem("isStreaming", _satellite.IsStreaming.ToString());
            msg.AddItem("isConnected", (!string.IsNullOrEmpty(_satellite.ServerId)).ToString());
            msg.AddItem("isConnecting", isConnecting ? bool.TrueString : bool.FalseString);

            SendMessage(msg);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in NotifyUiState");
        }
    }

    private Task NotifyError(Exception? ex)
    {
        try
        {
            if (_satellite == null || ApplicationHelper.CheckUiState() != ApplicationRunningContext.AppState.Foreground)
            {
                return Task.CompletedTask;
            }

            Bundle msg = new();
            msg.AddItem(Constants.Events.EventKey, Constants.Events.ErrorEvent);
            msg.AddItem("errorDetails", ex?.ToString());

            SendMessage(msg);
        }
        catch (Exception e)
        {
            TizenLogger.Singleton.LogError(e, "Error in NotifyError");
        }

        return Task.CompletedTask;
    }

    private void SendWakeWordDetected()
    {
        try
        {
            Bundle msg = new();
            msg.AddItem(Constants.Events.EventKey, Constants.Events.WakeWordDetectedEvent);

            SendMessage(msg);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in SendWakeWordDetected");
        }
    }

    private void ReplyToPing()
    {
        try
        {
            Bundle msg = new();
            msg.AddItem(Constants.Events.EventKey, Constants.Events.PongEvent);

            SendMessage(msg);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in ReplyToPing");
        }
    }

    private async Task StopSatellite()
    {
        try
        {
// #if DEBUG
//             TerminateProfiler();
// #endif

            if (TizenServer.Singleton != null)
            {
                await TizenServer.Singleton.StopAsync();
                NotifyUiState();
                TizenServer.Singleton = null;
                _satellite = null;
            }

            _speakerProvider?.Dispose();
            _speakerProvider = null;

            _audioFocusManager?.Dispose();
            _audioFocusManager = null;
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Error in StopSatellite");
        }
    }

#if DEBUG
    private static void LaunchProfiler()
    {
        try
        {
            var appControl = new AppControl { ApplicationId = Constants.ProfilerAppId };
            AppControl.SendLaunchRequest(appControl);
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Failed to launch profiler app");
        }
    }

    private static void TerminateProfiler()
    {
        try
        {
            var context = new ApplicationRunningContext(Constants.ProfilerAppId);
            context.Terminate();
        }
        catch (Exception ex)
        {
            TizenLogger.Singleton.LogError(ex, "Failed to terminate profiler app");
        }
    }
#endif

    private void SendMessage(Bundle msg)
    {
        if (ApplicationHelper.CheckUiState() != ApplicationRunningContext.AppState.Foreground)
        {
            return;
        }

        try
        {
            _localPort.Send(msg, Constants.UiAppId, Constants.UiPortName);
        }
        catch (Exception ex)
        {
            // Better to fail silently than crash the service?
            TizenLogger.Singleton.LogError(ex, "Failed to send message to UI");
        }
        finally
        {
            msg.Dispose();
        }
    }
}
