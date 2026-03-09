using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    private readonly MessagePort _uiLocalPort = new(Constants.UiPortName, false);

    private readonly ManualResetEventSlim pingEvent = new ManualResetEventSlim();

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

    protected override async Task LoopAsync()
    {
        const int defaultWaitTimeSeconds = 5;
        const int maxPatience = 5;
        int pingPatience = 0;

        while (!CancellationTokenSource!.IsCancellationRequested)
        {
            if (!IsServiceRunning())
            {
                LaunchBackground();
            }
            else if (_uiLocalPort is not null && _uiLocalPort.Listening)
            {
                SendCommandToService(Constants.Commands.PingCommand);

                if (pingEvent.Wait(TimeSpan.FromSeconds(2)))
                {
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
                        LaunchBackground();
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(defaultWaitTimeSeconds));
        }
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

            // TODO: remove this?
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

        SendCommandToService(Constants.Commands.GetStatusCommand);
    }

    private void SendCommandToService(string command)
    {
        using Bundle msg = new();
        msg.AddItem(Constants.Commands.CommandKey, command);
        _uiLocalPort.Send(msg, Constants.ServiceAppId, Constants.ServicePortName, false);
    }
}
