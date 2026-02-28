using Wyoming.Net.Satellite.App.Maui.Abstractions;
using Wyoming.Net.Satellite.App.Maui.ViewModels;

namespace Wyoming.Net.Satellite.App.Maui;

public partial class MainPage : ContentPage
{
    private readonly ISatelliteService satelliteService;
    private readonly SatelliteSettingsViewModel settingsViewModel;
    private readonly SatelliteStateViewModel stateViewModel;

    public MainPage(
        ISatelliteService satelliteService,
        SatelliteSettingsViewModel settingsViewModel
        )
    {
        InitializeComponent();
        this.satelliteService = satelliteService;
        this.settingsViewModel = settingsViewModel;
        this.stateViewModel = new SatelliteStateViewModel();

        BindingContext = this.stateViewModel;

        satelliteService.StateChanged += OnSatelliteStateChanged;
        satelliteService.ErrorOccurred += OnSatelliteError;
        satelliteService.WakeWordDetected += OnWakeWordDetected;

        SyncStateFromService();
    }

    private void SyncStateFromService()
    {
        stateViewModel.IsRunning = satelliteService.IsRunning;
        stateViewModel.IsStreaming = satelliteService.IsStreaming;
        stateViewModel.ServerConnected = satelliteService.ServerConnected;
        stateViewModel.IsPaused = satelliteService.IsPaused;
        stateViewModel.MicMuted = satelliteService.MicMuted;

        if (stateViewModel.IsRunning || satelliteService.IsRunning)
        {
            RunUIUpdate(() =>
            {
                StartStopButton.Text = "Stop Satellite";
                StartStopButton.Background = new SolidColorBrush(Colors.Red);
                ListeningAnimation.IsConnected = stateViewModel.ServerConnected;
                ListeningAnimation.IsListening = stateViewModel.IsStreaming;
            });
        }
    }

    private void OnSatelliteStateChanged()
    {
        stateViewModel.IsStreaming = satelliteService.IsStreaming;
        stateViewModel.IsRunning = satelliteService.IsRunning;
        stateViewModel.IsPaused = satelliteService.IsPaused;
        stateViewModel.MicMuted = satelliteService.MicMuted;
        stateViewModel.ServerConnected = satelliteService.ServerConnected;

        RunUIUpdate(() =>
        {
            ListeningAnimation.IsConnecting = stateViewModel.IsRunning && !stateViewModel.ServerConnected;
            ListeningAnimation.IsConnected = stateViewModel.ServerConnected;
            ListeningAnimation.IsListening = stateViewModel.IsStreaming;

            if (!stateViewModel.IsRunning)
            {
                StartStopButton.Text = "Start Satellite";
                StartStopButton.Background = new SolidColorBrush(Color.FromArgb("#4F46E5"));
                ListeningAnimation.IsConnecting = false;
                ListeningAnimation.IsConnected = false;
                ListeningAnimation.IsListening = false;
            }
        });
    }

    private void OnSatelliteError(Exception exception)
    {
        _ = RunUIUpdateAsync(async () =>
        {
            await DisplayAlert(
                "Satellite Error",
                exception.Message,
                "OK");
        });
    }

    private void OnWakeWordDetected()
    {
        // Wake word sound is now played by the foreground service
    }

    private async void OnStartStopClicked(object sender, EventArgs args)
    {
        await ToggleServer();
    }

    private async Task ToggleServer()
    {
        if (satelliteService.IsRunning || stateViewModel.IsRunning)
        {
            await StopServerAsync();
        }
        else
        {
            await StartServerAsync();
        }
    }

    private async Task StartServerAsync()
    {
        if (!settingsViewModel.IsValid(out var message))
        {
            await DisplayAlert(
                "Failed start satellite",
                message,
                "OK");
            return;
        }

        if (!await EnsureMicrophonePermissionAsync())
        {
            return;
        }

        if (!await EnsureNotificationPermissionAsync())
        {
            return;
        }

        ListeningAnimation.IsConnecting = true;

        await satelliteService.StartAsync();
        stateViewModel.IsRunning = true;

        RunUIUpdate(() =>
        {
            StartStopButton.Text = "Stop Satellite";
            StartStopButton.Background = new SolidColorBrush(Colors.Red);
        });
    }

    private async Task StopServerAsync()
    {
        await satelliteService.StopAsync();
        stateViewModel.IsRunning = false;

        RunUIUpdate(() =>
        {
            StartStopButton.Text = "Start Satellite";
            StartStopButton.Background = new SolidColorBrush(Color.FromArgb("#4F46E5"));
            ListeningAnimation.IsConnecting = false;
            ListeningAnimation.IsConnected = false;
            ListeningAnimation.IsListening = false;
        });
    }

    private async Task<bool> EnsureMicrophonePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        if (Permissions.ShouldShowRationale<Permissions.Microphone>())
        {
            await DisplayAlert(
                "Microphone permission",
                "This app needs access to the microphone to record audio.",
                "OK");
        }

        status = await Permissions.RequestAsync<Permissions.Microphone>();

        return status == PermissionStatus.Granted;
    }

    private async Task<bool> EnsureNotificationPermissionAsync()
    {
#if ANDROID
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

            if (status == PermissionStatus.Granted)
                return true;

            if (Permissions.ShouldShowRationale<Permissions.PostNotifications>())
            {
                await DisplayAlert(
                    "Notification permission",
                    "This app needs notification permission to run the satellite in the background.",
                    "OK");
            }

            status = await Permissions.RequestAsync<Permissions.PostNotifications>();

            return status == PermissionStatus.Granted;
        }
#endif
        return true;
    }

    private static void RunUIUpdate(Action action)
    {
        MainThread.BeginInvokeOnMainThread(action);
    }

    private static Task RunUIUpdateAsync(Func<Task> action)
    {
        return MainThread.InvokeOnMainThreadAsync(action);
    }
}
