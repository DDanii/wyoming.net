using System.Runtime.InteropServices;

namespace Wyoming.Net.Satellite.App.Tz.Platform.Interop;

internal static class NativeDisplay
{
    public const string Deviced = "/usr/lib/libdeviced.so.1";

    [DllImport(Deviced, CallingConvention = CallingConvention.Cdecl)]
    private static extern int device_set_screen_state(int onoff);


    public static bool TurnOffScreen()
    {
        try
        {
            return device_set_screen_state(0) == 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool TurnOnScreen()
    {
        try
        {
            return device_set_screen_state(1) == 0;
        }
        catch
        {
            return false;
        }
    }
}
