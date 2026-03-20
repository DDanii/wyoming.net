using System;
using System.Linq.Expressions;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;
using Wyoming.Net.Satellite.App.Tz.Components;

namespace Wyoming.Net.Satellite.App.Tz.Pages;

internal static class TizenUI
{
    public static TextLabel CreateLabel(string text)
    {
        var label = new TextLabel(text)
        {
            PointSize = 28,
            TextColor = new Color("#E5E7EB"),
            Margin = new Extents(0, 0, 0, 30),
            Focusable = false,
            HorizontalAlignment = HorizontalAlignment.Begin,
        };

        return label;
    }

    public static TextField CreateInput<TTarget, TData>(TTarget target, Func<TTarget, TData> getter, Action<TTarget, string> setter, bool isLastField = false)
    {
        var input = new TextField
        {
            PointSize = 28,
            WidthResizePolicy = ResizePolicyType.FillToParent,
            BackgroundColor = new Color("#1F2937"),
            Margin = new Extents(0, 0, 0, 40),
            BorderlineColor = new Color("#374151"),
            BorderlineWidth = 2,
            Padding = new Extents(30, 30, 20, 20),
            Text = getter(target)?.ToString(),
            Focusable = true,
            TextColor = new Color("#E5E7EB")
        };

        // Configure Next vs Done action button
        var inputMethod = new InputMethod
        {
            ActionButton = isLastField
                ? InputMethod.ActionButtonTitleType.Done
                : InputMethod.ActionButtonTitleType.Next
        };
        input.InputMethodSettings = inputMethod.OutputMap;

        // Enable auto-show of IME on focus
        var imContext = input.GetInputMethodContext();
        imContext.AutoEnableInputPanel(true);

        // Handle Return key from IME to move to next field
        input.KeyEvent += (s, e) =>
        {
             if (e.Key.State == Key.StateType.Down
                && e.Key.KeyPressedName == "Back")
            {
                imContext.Deactivate();
                imContext.HideInputPanel();
                return true;
            }

            if (e.Key.State == Key.StateType.Down
                && e.Key.KeyPressedName == "Select")
            {
                imContext.Deactivate();
                imContext.HideInputPanel();

                if (!isLastField && input.DownFocusableView != null)
                {
                    FocusManager.Instance.SetCurrentFocusView(input.DownFocusableView);
                }

                return true; // consumed
            }
            return false;
        };

        input.FocusGained += (s, e) =>
        {
            input.BorderlineColor = new Color("#6366F1");
            input.BackgroundColor = new Color("#111827");
            input.Scale = new Vector3(1.05f, 1.05f, 1);
        };

        input.FocusLost += (s, e) =>
        {
            input.BorderlineColor = new Color("#374151");
            input.BackgroundColor = new Color("#1F2937");
            input.Scale = Vector3.One;
        };


        input.TextChanged += (s, args) =>
        {
            setter(target, args.TextField.Text);
        };

        return input;
    }

    public static Button CreateToggle<TTarget>(TTarget target, Func<TTarget, bool> getter, Action<TTarget, bool> setter)
    {
        bool currentValue = getter(target);

        var toggle = new Button
        {
            Text = currentValue ? "On" : "Off",
            Focusable = true,
            WidthResizePolicy = ResizePolicyType.FillToParent,
            HeightSpecification = 70,
            Margin = new Extents(0, 0, 0, 40),
            BorderlineWidth = 2,
            BorderlineColor = TvStyle.ButtonBorderlineColor,
            BackgroundColor = currentValue ? new Color("#1F2937") : new Color("#DC2626"),
            TextColor = Color.White,
        };

        toggle.Clicked += (s, e) =>
        {
            bool newValue = !getter(target);
            setter(target, newValue);
            toggle.Text = newValue ? "On" : "Off";
            toggle.BackgroundColor = newValue ? new Color("#1F2937") : new Color("#DC2626");
        };

        toggle.FocusGained += (s, e) =>
        {
            toggle.BorderlineColor = TvStyle.ButtonFocusedBorderlineColor;
            toggle.Scale = new Vector3(1.05f, 1.05f, 1);
        };

        toggle.FocusLost += (s, e) =>
        {
            toggle.BorderlineColor = TvStyle.ButtonBorderlineColor;
            toggle.Scale = Vector3.One;
        };

        return toggle;
    }
}
