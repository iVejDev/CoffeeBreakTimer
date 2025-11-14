using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

/*
    En enkel in-memory implementation av ISettingsRepository för att lagra timerinställningar.
    Denna implementation lagrar inställningarna i minnet och förlorar dem när applikationen stängs.
*/
public class InMemorySettingsRepository : ISettingsRepository
{
    private TimerSettings _settings = new() { WorkMinutes = 25, BreakMinutes = 5 };

    public TimerSettings Load() => _settings;

    public void Save(TimerSettings settings) => _settings = settings;
}
