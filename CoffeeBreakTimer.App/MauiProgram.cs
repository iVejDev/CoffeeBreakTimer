using CoffeeBreakTimer.App.DI;
using Microsoft.Extensions.Logging;

namespace CoffeeBreakTimer.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
            });

        builder.ConfigureServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
