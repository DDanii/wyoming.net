using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Wyoming.Net.Satellite.App.Tz.Platform;


namespace Wyoming.Net.Satellite.App.Tz
{
    public static class Program
    {
        static void Main(string[] args)
        {
            RemoteLogger.InitSingleton("192.168.1.148", 5005);

           var app = string.Empty;
            
            for(int i = 0; i < args.Length; i++)
            {
                if(args[i] == "__APP_SVC_PKG_NAME__")
                {
                    app = Encoding.UTF8.GetString(Convert.FromBase64String(args[i + 1]));
                }
            }
            
            TizenLogger.Singleton.LogInformation("Initializing app: {app}", app);

            if (app.EndsWith(Constants.ServiceAppId))
            {
                var service = new BackgroundApp();
                service.Run(args);
            }
            else if (app.EndsWith(Constants.ProfilerAppId))
            {
                var profiler = new ProfilerApp();
                profiler.Run(args);
            }
            else
            {
                var ui = new UIApp();
                ui.Run(args);
            }
        }
    }
}