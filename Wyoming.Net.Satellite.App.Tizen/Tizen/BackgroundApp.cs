using System;
using System.Text.Json;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Applications.Messages;
using Tizen.System;
using Wyoming.Net.Satellite.App.Tz.Platform;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz;

public sealed class BackgroundApp : ServiceApplication
{
    private MessagePort _localPort = new MessagePort(Constants.ServicePortName, false);
    private TizenSpeakerProvider _speakerProvider = new TizenSpeakerProvider();
    private WakeWordSatellite? _satellite;

    protected override void OnCreate()
    {
        base.OnCreate();

        ManagePowerLock(true);

        _localPort.MessageReceived += OnMessageReceived;
        _localPort.Listen();
    }

    private static void ManagePowerLock(bool acquiring)
    {
        try
        {
            if(acquiring)
            {
                Power.RequestLock(PowerLock.Cpu, 0);
            }
            else
            {
                Power.ReleaseLock(PowerLock.Cpu);
            }
        }
        catch
        {
        }
    }

    protected override void OnTerminate()
    {
        try
        {
            ManagePowerLock(false);
            StopSatellite().Wait(TimeSpan.FromSeconds(5));
        }
        finally
        {
            base.OnTerminate();
        }
    }

    protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
    {
        ReceivedAppControl receivedAppControl = e.ReceivedAppControl;

        if (receivedAppControl.IsReplyRequest)
        {
            AppControl replyRequest = new();
            receivedAppControl.ReplyToLaunchRequest(replyRequest, AppControlReplyResult.Succeeded);
        }

        base.OnAppControlReceived(e);
    }

    private async void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
    {
        var command = args.Message.GetItem<string>(Constants.Commands.CommandKey);

        switch (command)
        {
            case Constants.Commands.StartCommand:
                await StartSatellite();
                break;
            case Constants.Commands.StopCommand:
                await StopSatellite();
                break;
            case Constants.Commands.GetStatusCommand:
                NotifyUiState();
                break;
        }
    }

    private async Task StartSatellite()
    {
        if (_satellite != null && _satellite.IsRunning)
        {
            return;
        }

        var settingsViewModel = SatelliteSettingsViewModel.Load();
        var settings = settingsViewModel.ToSatelliteSettings();
        var wakeModels = await settingsViewModel.WakeSettings.GetModelsAsync();
        var loggerFactory = new TizenLoggerFactory();

        _satellite = new WakeWordSatellite(settings, wakeModels, loggerFactory, new TizenMicProvider(), _speakerProvider);
        _satellite.StateChanged += () => NotifyUiState();
        _satellite.WakeWordDetected += HandleWakeWordDetected;
        _satellite.SatelliteError += NotifyError;

        TizenServer.CreateSingleton(_satellite, settingsViewModel, loggerFactory);
        await TizenServer.Singleton!.StartAsync();

        NotifyUiState();
    }

    private async Task HandleWakeWordDetected()
    {
        var wav = await TizenAssetReader.ReadAssetAsync("ww_detected3.wav");
        var wavInfo = WavHelper.ReadWavInfo(wav);
        await _speakerProvider.StartAsync(wavInfo.SampleRate, wavInfo.BytesPerSample, wavInfo.Channels);
        await _speakerProvider.PlayAsync(wav, null);
        await _speakerProvider.StopAsync();

        SendWakeWordDetected();
    }

    private void NotifyUiState()
    {
        if (_satellite == null || CheckUiState() != ApplicationRunningContext.AppState.Foreground)
        {
            return;
        }

        Bundle msg = new();
        msg.AddItem(Constants.Events.EventKey, Constants.Events.StateChangedEvent);
        msg.AddItem("isRunning", _satellite.IsRunning.ToString());
        msg.AddItem("isStreaming", _satellite.IsStreaming.ToString());
        msg.AddItem("isConnected", (!string.IsNullOrEmpty(_satellite.ServerId)).ToString());

        SendMessage(msg);
    }

    private Task NotifyError(Exception? ex)
    {
        if (_satellite == null || CheckUiState() != ApplicationRunningContext.AppState.Foreground)
        {
            return Task.CompletedTask;
        }

        Bundle msg = new();
        msg.AddItem(Constants.Events.EventKey, Constants.Events.ErrorEvent);
        msg.AddItem("errorDetails", ex?.ToString());

        SendMessage(msg);

        return Task.CompletedTask;
    }

    private void SendWakeWordDetected()
    {
        Bundle msg = new();
        msg.AddItem(Constants.Events.EventKey, Constants.Events.WakeWordDetectedEvent);

        SendMessage(msg);
    }

    private async Task StopSatellite()
    {
        if (TizenServer.Singleton != null)
        {
            await TizenServer.Singleton.StopAsync();
            TizenServer.Singleton = null;
            _satellite = null;
        }

        NotifyUiState();
    }

    private static ApplicationRunningContext.AppState CheckUiState()
    {
        try
        {
            using var context = new ApplicationRunningContext(Constants.UiAppId);
            return context.State;
        }
        catch
        {
            return ApplicationRunningContext.AppState.Undefined;
        }
    }

    private void SendMessage(Bundle msg)
    {
        if(CheckUiState() != ApplicationRunningContext.AppState.Foreground)
        {
            return;
        }
        
        try
        {
            _localPort.Send(msg, Constants.UiAppId, Constants.UiPortName);
        }
        catch
        {
            // Better to fail silently than crash the service?
        }
        finally
        {
            msg.Dispose();
        }
    }
}
