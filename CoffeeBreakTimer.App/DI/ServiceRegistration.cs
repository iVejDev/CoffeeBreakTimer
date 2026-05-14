using CoffeeBreakTimer.App.Services;
using CoffeeBreakTimer.App.ViewModels;
using CoffeeBreakTimer.App.Views;
using CoffeeBreakTimer.Core.Interfaces;
using CoffeeBreakTimer.Core.Services;

namespace CoffeeBreakTimer.App.DI;

public static class ServiceRegistration
{
    public static void ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ITimerService, MauiTimerService>();
        builder.Services.AddSingleton<IAudioPlayer, MauiAudioPlayer>();
        builder.Services.AddSingleton<IAmbiencePlayer, MauiAmbiencePlayer>();
        builder.Services.AddSingleton<ISettingsRepository, PreferencesSettingsRepository>();
        builder.Services.AddSingleton<IAppPreferencesRepository, PreferencesAppPreferencesRepository>();
        builder.Services.AddSingleton<ITaskRepository, JsonTaskRepository>();
        builder.Services.AddSingleton<IStatisticsRepository, JsonStatisticsRepository>();
        builder.Services.AddSingleton<CoffeeTimerService>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
    }
}
