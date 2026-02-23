using System;
using System.Threading;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Applications.Messages;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Wyoming.Net.Satellite.App.Tz.Components;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz.Pages;

public class MainPage : View
{
    private ListeningAnimationComponent listeningAnimationComponent;

    private MessagePort _uiLocalPort = new(Constants.UiPortName, false);

    private SatelliteStateViewModel stateViewModel = new();

    private SatelliteButton startStopButton;

    private readonly View parent;
    private readonly SynchronizationContext uiContext;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MainPage(View parent)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        this.parent = parent;
        uiContext = TizenSynchronizationContext.Current!;

        InitializeUI();
        LaunchBackgrund();
    }

    private void InitializeUI()
    {
        Focusable = true;
        FocusGained += OnFocus;

        var view = new View
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent,
            Padding = new Extents(0, 0, 70, 0),
            Layout = new LinearLayout()
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };

        var title = TizenUI.CreateLabel("Wyoming .NET");
        title.PointSize = 40;
        title.Padding = new Extents(0, 0, 40, 40);
        title.TextColor = Color.White;

        listeningAnimationComponent = new ListeningAnimationComponent()
        {
            Margin = new Extents(0, 0, 40, 60)
        };


        startStopButton = new SatelliteButton
        {
            UpFocusableView = parent,
        };
        startStopButton.Clicked += async (s, args) => await ToggleServer();

        view.Add(title);
        view.Add(listeningAnimationComponent);
        view.Add(startStopButton);

        Add(view);
    }

    private void OnFocus(object? sender, EventArgs args)
    {
        FocusManager.Instance.SetCurrentFocusView(startStopButton);
    }

    private void InitializeCommunication()
    {
        _uiLocalPort.MessageReceived += (s, e) =>
        {
            string eventName = e.Message.GetItem<string>(Constants.Events.EventKey);
            RunUIUpdate(() => HandleServiceEvent(eventName, e.Message));
        };

        _uiLocalPort.Listen();

        SendCommandToService(Constants.Commands.GetStatusCommand);
    }

    private void AppControlReplyCallback(AppControl launchRequest, AppControl replyRequest, AppControlReplyResult result)
    {
        if (result >= AppControlReplyResult.Succeeded)
        {
            InitializeCommunication();
        }
        else
        {
            TvDialog.ShowOkDialog("Ops", "Failed to start background service");
        }
    }

    private void LaunchBackgrund()
    {
        AppControl serviceLaunchRequest = new()
        {
            ApplicationId = Constants.ServiceAppId,
            Operation = AppControlOperations.Default
        };

        AppControl.SendLaunchRequest(serviceLaunchRequest, 0, AppControlReplyCallback);
    }

    private void HandleServiceEvent(string eventName, Bundle data)
    {
        if (eventName == Constants.Events.StateChangedEvent)
        {
            listeningAnimationComponent.IsConnecting = false;
            listeningAnimationComponent.IsConnected = bool.Parse(data.GetItem<string>("isConnected"));
            listeningAnimationComponent.IsListening = bool.Parse(data.GetItem<string>("isStreaming"));

            bool isRunning = bool.Parse(data.GetItem<string>("isRunning"));

            if (isRunning != stateViewModel.IsRunning)
            {
                stateViewModel.IsRunning = isRunning;
                startStopButton.FlipState();
            }

            return;
        }

        if(eventName == Constants.Events.ErrorEvent)
        {
            OnSatelliteError(data.GetItem<string>("errorDetails"));
        }
    }

    private void SendCommandToService(string command)
    {
        using Bundle msg = new();
        msg.AddItem(Constants.Commands.CommandKey, command);
        _uiLocalPort.Send(msg, Constants.ServiceAppId, Constants.ServicePortName, false);
    }

    private async Task ToggleServer()
    {
        SendCommandToService(stateViewModel.IsRunning ? Constants.Commands.StopCommand : Constants.Commands.StartCommand);
    }

    private void OnSatelliteError(string? details)
    {
        RunUIUpdate(async () =>
        {
            TvDialog.ShowOkDialog("Ops", $"Error from satellite: {details}");
        });
    }

    private async Task StopServerAsync()
    {
        SendCommandToService(Constants.Commands.StopCommand);
    }

    private void RunUIUpdate(Action action)
    {
        uiContext.Post((_) => action(), null);
    }
}