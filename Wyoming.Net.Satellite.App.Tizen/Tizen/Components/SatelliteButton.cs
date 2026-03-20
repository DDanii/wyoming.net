using Tizen.NUI;
using Tizen.NUI.Components;

namespace Wyoming.Net.Satellite.App.Tz.Components;

internal sealed class SatelliteButton : Button
{
    enum SatelliteButtonState
    {
        Paused,
        Started
    }

    private SatelliteButtonState _state;

    private readonly string startText;

    private readonly string stopText;

    public SatelliteButton(string startText = "Start Satellite", string stopText = "Stop Satellite")
    {
        this.startText = startText;
        this.stopText = stopText;

        Text = startText;
        Focusable = true;
        FocusNavigationSupport = true;
        BorderlineColor = TvStyle.ButtonBorderlineColor;
        BorderlineWidth = 2;
        BackgroundColor = TvStyle.ButtonBackgroundColor;
        TextColor = Color.White;
        Margin = new Extents(0, 0, 60, 0);
        Padding = new Extents(20, 20, 20, 20);
        CellVerticalAlignment = VerticalAlignmentType.Center;
        _state = SatelliteButtonState.Paused;
    }

    public override void OnFocusGained()
    {
        Scale = new Vector3(1.12f, 1.12f, 1);

        if (_state != SatelliteButtonState.Started)
        {
            BackgroundColor = TvStyle.ButtonFocusedBackgroundColor;
            BorderlineColor = TvStyle.ButtonFocusedBorderlineColor;
        }

        base.OnFocusGained();
    }

    public override void OnFocusLost()
    {

        Scale = Vector3.One;

        if (_state != SatelliteButtonState.Started)
        {
            BackgroundColor = TvStyle.ButtonBackgroundColor;
            BorderlineColor = TvStyle.ButtonBorderlineColor;
        }

        base.OnFocusLost();
    }

    public void FlipState()
    {
        if (_state == SatelliteButtonState.Started)
        {
            StopState();
        }
        else
        {
            StartState();
        }
    }

    public void StopState()
    {
        _state = SatelliteButtonState.Paused;

        Text = startText;
        BackgroundColor = TvStyle.ButtonBackgroundColor;
    }

    public void StartState()
    {
        _state = SatelliteButtonState.Started;

        Text = stopText;
        BackgroundColor = Color.Red;
    }
}
