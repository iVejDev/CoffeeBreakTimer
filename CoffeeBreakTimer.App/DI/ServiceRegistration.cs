using CoffeeBreakTimer.App.Services;
using CoffeeBreakTimer.Core.Interfaces;
using CoffeeBreakTimer.Core.Services;

namespace CoffeeBreakTimer.App.DI;

/*
 * This static class is responsible for registering services into the dependency injection container.
 * It adds singleton instances of various services used throughout the application.
 */
public static class ServiceRegistration
{
    public static void ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ITimerService, MauiTimerService>();
        builder.Services.AddSingleton<IAudioPlayer, MauiAudioPlayer>();
        builder.Services.AddSingleton<ISettingsRepository, InMemorySettingsRepository>();
        builder.Services.AddSingleton<CoffeeTimerService>();
    }
}
