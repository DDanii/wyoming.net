using Wyoming.Net.Satellite.App.Maui;
using Wyoming.Net.Satellite.App.Maui.Abstractions;
using Wyoming.Net.Satellite.App.Maui.ViewModels;

namespace Wyoming.Net.Satellite.App.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseSharedMauiApp();

            builder.Services.AddSingleton<ISatelliteService>(sp =>
                new DroidSatelliteService(
                    sp.GetRequiredService<SatelliteSettingsViewModel>()));

            return builder.Build(); 
        }
    }
}
