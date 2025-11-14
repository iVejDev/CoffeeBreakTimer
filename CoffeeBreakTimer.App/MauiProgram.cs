using CoffeeBreakTimer.App.DI;

namespace CoffeeBreakTimer.App;

/*
 * This static class is responsible for creating and configuring the MAUI application.
 * It sets up the application builder and integrates service registrations.
 */
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        builder.ConfigureServices();

        return builder.Build();
    }
}
