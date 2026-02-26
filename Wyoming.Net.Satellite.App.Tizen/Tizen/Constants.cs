using System;

namespace Wyoming.Net.Satellite.App.Tz;

internal static class Constants
{
    public const string UiAppId = "com.guilhermepohlmann.wyoming.net";

    public const string UiPortName = "wyoming.net.ui";

    public const string ServiceAppId = "com.guilhermepohlmann.wyoming.net.service";

    public const string ServicePortName = "wyoming.net.server";

    public static class Events
    {
        public const string EventKey = "event";

        public const string WakeWordDetectedEvent = "WAKE_WORD_DETECTED";

        public const string StateChangedEvent = "STATE_CHANGED";

        public const string ErrorEvent = "ERROR";

        public const string PongEvent = "PONG";
    }

    public static class Commands
    {
          public const string CommandKey = "command";

          public const string StartCommand = "START";

          public const string StopCommand = "STOP";

          public const string GetStatusCommand = "GET_STATUS";

          public const string PingCommand = "PING";
    }
}
