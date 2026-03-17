namespace Wyoming.Net.Satellite.App.Tz.ViewModels;

public sealed class PowerStateSettingsViewModel
{
    public bool MotionSensorEnabled { get; set; } = Tizen.TV.System.Sensor.MotionSensor.IsSupported;

    public bool TurnOffScreen { get; set; } = true;

    public int NoMotionTimeoutSeconds { get; set; } = 300;
}
