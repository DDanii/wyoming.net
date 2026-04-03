using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Applications.Messages;
using Wyoming.Net.Core;
using Wyoming.Net.Satellite.App.Tz.Components;

namespace Wyoming.Net.Satellite.App.Tz.Platform;

internal static class ApplicationHelper
{
    public static ApplicationRunningContext.AppState CheckUiState()
    {
        return CheckAppState(Constants.UiAppId);
    }

    public static ApplicationRunningContext.AppState CheckServiceState()
    {
        return CheckAppState(Constants.ServiceAppId);
    }

    public static ApplicationRunningContext.AppState KillService()
    {
        try
        {
            using var context = new ApplicationRunningContext(Constants.ServiceAppId);
            ApplicationManager.TerminateBackgroundApplication(context);

            return context.State;
        }
        catch
        {
            return ApplicationRunningContext.AppState.Undefined;
        }
    }

    private static ApplicationRunningContext.AppState CheckAppState(string appId)
    {
        try
        {
            using var context = new ApplicationRunningContext(appId);
            return context.State;
        }
        catch
        {
            return ApplicationRunningContext.AppState.Undefined;
        }
    }
}

internal sealed class ServiceManager : TaskLoopRunner
{
    private static readonly ServiceManager _singleton = new();

    private MessagePort _uiLocalPort = new(Constants.UiPortName, false);

    private readonly ManualResetEventSlim pingEvent = new ManualResetEventSlim();

    private bool isCommunicating;

    private ServiceManager() : base(new TizenLogger(), TaskLoopRunnerOptions.LongRunning)
    {
    }

    public static ServiceManager Singleton => _singleton;

    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    public void SendStartSatellite()
    {
        SendCommandToService(Constants.Commands.StartCommand);
    }

    public void SendStopSatellite()
    {
        SendCommandToService(Constants.Commands.StopCommand);
    }

    public void SendReloadSettings()
    {
        SendCommandToService(Constants.Commands.ReloadSettingsCommand);
    }

    public void SendGetStatus()
    {
        SendCommandToService(Constants.Commands.GetStatusCommand);
    }

    public async Task KillService()
    {
        await StopAsync();
        ApplicationHelper.KillService();
    }

    public bool IsCommunicating => isCommunicating;

    protected override async Task LoopAsync()
    {
        const int defaultWaitTimeSeconds = 5;
        const int maxPatience = 5;
        int pingPatience = 0;
        Stopwatch watch = new Stopwatch();

        while (!CancellationTokenSource!.IsCancellationRequested)
        {
            if (!IsServiceRunning())
            {
                if (!watch.IsRunning || watch.Elapsed.Seconds > defaultWaitTimeSeconds)
                {
                    watch.Start();
                    LaunchBackground();
                }
            }
            else if (_uiLocalPort is not null && _uiLocalPort.Listening)
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }

                SendCommandToService(Constants.Commands.PingCommand);

                if (pingEvent.Wait(TimeSpan.FromSeconds(2)))
                {
                    isCommunicating = true;

                    pingPatience = 0;
                    pingEvent.Reset();
                }
                else
                {
                    if (pingPatience < maxPatience)
                    {
                        pingPatience++;

                        await Task.Delay(TimeSpan.FromSeconds(defaultWaitTimeSeconds + pingPatience));
                        continue;
                    }
                    else
                    {
                        // Force a relaunch
                        pingPatience = 0;
                        isCommunicating = false;
                        LaunchBackground();
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(defaultWaitTimeSeconds));
        }
    }

    protected override ValueTask OnStartAsync()
    {
        _uiLocalPort = new(Constants.UiPortName, false);
        return base.OnStartAsync();
    }

    protected override async ValueTask OnStopAsync()
    {
        await base.OnStopAsync();

        isCommunicating = false;
        _uiLocalPort.Dispose();
    }

    private static bool IsServiceRunning()
    {
        return ApplicationHelper.CheckServiceState() == ApplicationRunningContext.AppState.Service;
    }

    private void LaunchBackground()
    {
        AppControl serviceLaunchRequest = new()
        {
            ApplicationId = Constants.ServiceAppId,
            Operation = AppControlOperations.Default
        };

        AppControl.SendLaunchRequest(serviceLaunchRequest, 0, AppControlReplyCallback);
    }

    private void AppControlReplyCallback(AppControl launchRequest, AppControl replyRequest, AppControlReplyResult result)
    {
        if (result >= AppControlReplyResult.Succeeded)
        {
            InitializeCommunication();

            SendCommandToService(Constants.Commands.StartCommand);
        }
        else
        {
            TvDialog.ShowOkDialog("Ops", "Failed to start background service");
        }
    }

    private void InitializeCommunication()
    {
        _uiLocalPort.MessageReceived += (s, e) =>
        {
            string eventName = e.Message.GetItem<string>(Constants.Events.EventKey);

            if (eventName == Constants.Events.PongEvent)
            {
                pingEvent.Set();
                return;
            }

            MessageReceived?.Invoke(s, e);
        };

        _uiLocalPort.Listen();
        isCommunicating = true;

        SendCommandToService(Constants.Commands.GetStatusCommand);
    }

    private void SendCommandToService(string command)
    {
        if (!isCommunicating)
        {
            return;
        }

        using Bundle msg = new();
        msg.AddItem(Constants.Commands.CommandKey, command);
        _uiLocalPort.Send(msg, Constants.ServiceAppId, Constants.ServicePortName, false);
    }
}
