using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace Wyoming.Net.Satellite.App.Droid;

public static class SatelliteNotificationHelper
{
    private const string ChannelId = "wyoming_satellite_channel";
    public const int NotificationId = 1;

    public const string ActionStop = "Wyoming.Net.Satellite.ACTION_STOP";

    public static void CreateNotificationChannel(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var channel = new NotificationChannel(
            ChannelId,
            "Wyoming Satellite",
            NotificationImportance.Low)
        {
            Description = "Keeps the Wyoming satellite running in the background"
        };

        channel.SetShowBadge(false);

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.CreateNotificationChannel(channel);
    }

    public static Notification BuildNotification(Context context, string contentText)
    {
        var launchIntent = context.PackageManager!
            .GetLaunchIntentForPackage(context.PackageName!)!;
        
        launchIntent.SetFlags(ActivityFlags.SingleTop);

        var contentPendingIntent = PendingIntent.GetActivity(
            context, 0, launchIntent, PendingIntentFlags.Immutable)!;

        var stopIntent = new Intent(context, typeof(SatelliteForegroundService));
        stopIntent.SetAction(ActionStop);

        var stopPendingIntent = PendingIntent.GetService(
            context, 1, stopIntent, PendingIntentFlags.Immutable)!;

        return new NotificationCompat.Builder(context, ChannelId)!
            .SetContentTitle("Wyoming Satellite")!
            .SetContentText(contentText)!
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)!
            .SetOngoing(true)!
            .SetContentIntent(contentPendingIntent)!
            .AddAction(Android.Resource.Drawable.IcMenuCloseClearCancel, "Stop", stopPendingIntent)!
            .SetForegroundServiceBehavior(NotificationCompat.ForegroundServiceImmediate)!
            .Build()!;
    }

    public static void UpdateNotification(Context context, string contentText)
    {
        var notification = BuildNotification(context, contentText);
        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.Notify(NotificationId, notification);
    }
}
