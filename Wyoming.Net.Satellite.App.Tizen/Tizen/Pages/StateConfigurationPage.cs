using System.Collections.Generic;
using System.Linq;
using Tizen.Applications;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;
using Wyoming.Net.Satellite.App.Tz.Components;
using Wyoming.Net.Satellite.App.Tz.ViewModels;

namespace Wyoming.Net.Satellite.App.Tz.Pages;

public class StateConfigurationPage : ContentPage
{
    public StateConfigurationPage(SatelliteSettingsViewModel vm, View parent, IEnumerable<ApplicationInfo> installedApps)
    {
        var scrollable = new ScrollableBase
        {
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightResizePolicy = ResizePolicyType.FillToParent,
            ScrollingDirection = ScrollableBase.Direction.Vertical,
            Padding = new Extents(200, 200, 20, 20),
            Layout = new LinearLayout
            {
                LinearOrientation = LinearLayout.Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
            }
        };

        var description = new TextLabel("Mark apps as Inactive to automatically stop the satellite when they are in the foreground.")
        {
            PointSize = 22,
            TextColor = new Color("#9CA3AF"),
            Margin = new Extents(0, 0, 0, 30),
            Focusable = false,
            MultiLine = true,
            WidthResizePolicy = ResizePolicyType.FillToParent,
        };
        scrollable.Add(description);

        var intervalLabel = TizenUI.CreateLabel("Watcher Interval (seconds)");
        var intervalInput = TizenUI.CreateInput(vm.StateConfiguration, (it) => it.WatcherIntervalSeconds, (it, value) => it.WatcherIntervalSeconds = value.ToIntOrDefault());
        intervalInput.UpFocusableView = parent;

        scrollable.Add(intervalLabel);
        scrollable.Add(intervalInput);

        var apps = installedApps.Where(a => !a.IsNoDisplay
                        && a.ApplicationId != Constants.UiAppId
                        && a.ApplicationId != Constants.ServiceAppId
                        && a.ApplicationId != Constants.ProfilerAppId
                        && !string.IsNullOrEmpty(a.ApplicationId))
            .OrderBy(a => a.Label ?? a.ApplicationId)
            .ToList();

        Button? firstButton = null;
        Button? previousButton = null;

        foreach (var appInfo in apps)
        {
            var row = new View
            {
                WidthResizePolicy = ResizePolicyType.FillToParent,
                HeightSpecification = 80,
                Margin = new Extents(0, 0, 0, 10),
                Layout = new LinearLayout
                {
                    LinearOrientation = LinearLayout.Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Begin,
                    CellPadding = new Size2D(20, 0),
                }
            };

            var label = new TextLabel(appInfo.ApplicationId + (string.IsNullOrEmpty(appInfo.Label) ? string.Empty : $" ({appInfo.Label})"))
            {
                PointSize = 24,
                TextColor = new Color("#E5E7EB"),
                Focusable = false,
                WidthSpecification = 800,
                VerticalAlignment = VerticalAlignment.Center,
            };

            string capturedAppId = appInfo.ApplicationId;
            bool isUnactive = vm.StateConfiguration.UnactiveApps.Contains(capturedAppId);

            var toggleBtn = new Button
            {
                Text = isUnactive ? "Inactive" : "Active",
                Focusable = true,
                WidthSpecification = 250,
                HeightSpecification = 70,
                BorderlineWidth = 2,
                BorderlineColor = TvStyle.ButtonBorderlineColor,
                BackgroundColor = isUnactive ? new Color("#DC2626") : new Color("#1F2937"),
                TextColor = Color.White,
            };

            toggleBtn.Clicked += (s, e) =>
            {
                if (vm.StateConfiguration.UnactiveApps.Contains(capturedAppId))
                {
                    vm.StateConfiguration.UnactiveApps.Remove(capturedAppId);
                    toggleBtn.Text = "Active";
                    toggleBtn.BackgroundColor = new Color("#1F2937");
                }
                else
                {
                    vm.StateConfiguration.UnactiveApps.Add(capturedAppId);
                    toggleBtn.Text = "Inactive";
                    toggleBtn.BackgroundColor = new Color("#DC2626");
                }
            };

            toggleBtn.FocusGained += (s, e) =>
            {
                toggleBtn.BorderlineColor = TvStyle.ButtonFocusedBorderlineColor;
                toggleBtn.Scale = new Vector3(1.05f, 1.05f, 1);
            };

            toggleBtn.FocusLost += (s, e) =>
            {
                toggleBtn.BorderlineColor = TvStyle.ButtonBorderlineColor;
                toggleBtn.Scale = Vector3.One;
            };

            if (previousButton != null)
            {
                toggleBtn.UpFocusableView = previousButton;
                previousButton.DownFocusableView = toggleBtn;
            }
            else
            {
                toggleBtn.UpFocusableView = intervalInput;
                intervalInput.DownFocusableView = toggleBtn;
                firstButton = toggleBtn;
            }

            row.Add(label);
            row.Add(toggleBtn);
            scrollable.Add(row);
            previousButton = toggleBtn;
        }

        Content = scrollable;
        Focusable = true;

        FocusGained += (s, args) =>
        {
            FocusManager.Instance.SetCurrentFocusView(intervalInput);
        };
    }
}
