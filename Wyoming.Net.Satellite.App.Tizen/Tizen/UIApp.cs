using System;
using Tizen.Applications;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Wyoming.Net.Satellite.App.Tz.Components;
using Wyoming.Net.Satellite.App.Tz.Pages;
using Wyoming.Net.Satellite.App.Tz.Platform;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz;

public sealed class UIApp : NUIApplication
{
    protected override async void OnPause()
    {
        if(ServiceManager.Singleton.IsRunning)
        {
           await ServiceManager.Singleton.StopAsync();
        }
        base.OnPause();
    }

    protected override async void OnResume()
    {
        await ServiceManager.Singleton.StartAsync();
        base.OnResume();
    }

    protected override async void OnCreate()
    {
        var settings = SatelliteSettingsViewModel.Load();
        RemoteLogger.InitSingleton(
            settings.ControlPanel.RemoteLogIp,
            settings.ControlPanel.RemoteLogPort);

        await ServiceManager.Singleton.StartAsync();
        FocusManager.Instance.FocusIndicator = null;

        var container = new View
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent,
            Focusable = true,
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Begin,
                VerticalAlignment = VerticalAlignment.Top,

            },
            BackgroundColor = TvStyle.MainBackgroundColor
        };

        var tabView = new TvTabView();

        var satelliteSettingsVm = SatelliteSettingsViewModel.Load();

        var main = new MainPage(tabView)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        SatelliteSettingsPage satelliteSettingsPage = new SatelliteSettingsPage(satelliteSettingsVm, tabView)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var wakeSettingsPage = new WakeSettingsPage(satelliteSettingsVm.WakeSettings, tabView)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var vadSettingsPage = new VadSettingsPage(satelliteSettingsVm.VadSettings, tabView)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var stateConfigPage = new StateConfigurationPage(satelliteSettingsVm, tabView, await ApplicationManager.GetInstalledApplicationsAsync())
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var powerStatePage = new PowerStateSettingsPage(satelliteSettingsVm.PowerStateSettings, tabView)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var controlPanelPage = new ControlPanelPage(satelliteSettingsVm.ControlPanel, tabView.Body)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var debugAudioPage = new DebugAudioPage(tabView.Body)
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent
        };

        var assistantTab = tabView.AddTab("Assistant", main);
        var controlPanelTab = tabView.AddTab("Control Panel", controlPanelPage);
        var satelliteSettingsTab = tabView.AddTab("Satellite Settings", satelliteSettingsPage);
        var wakeSettingsTab = tabView.AddTab("Wake Settings", wakeSettingsPage);
        var vadSettingsTab = tabView.AddTab("VAD Settings", vadSettingsPage);
        var stateConfigTab = tabView.AddTab("App States", stateConfigPage);
        var powerStateTab = tabView.AddTab("Power States", powerStatePage);
        var debugTab = tabView.AddTab("Debug", debugAudioPage);

        assistantTab.Leave += OnTabLeave;
        controlPanelTab.Leave += OnTabLeave;
        satelliteSettingsTab.Leave += OnTabLeave;
        wakeSettingsTab.Leave += OnTabLeave;
        vadSettingsTab.Leave += OnTabLeave;
        stateConfigTab.Leave += OnTabLeave;
        powerStateTab.Leave += OnTabLeave;
        debugTab.Leave += OnTabLeave;

        container.Add(tabView);
        Window.Instance.Add(container);

        FocusManager.Instance.SetCurrentFocusView(tabView);

        base.OnCreate();

        return;

        void OnTabLeave(object? sender, EventArgs args)
        {
            satelliteSettingsVm.Save();
            //ServiceManager.Singleton.SendReloadSettings();
        }
    }

    public async void OnKeyEvent(object sender, Window.KeyEventArgs e)
    {
        if (e.Key.State == Key.StateType.Down && (e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
        {
            Exit();
        }
    }
}
