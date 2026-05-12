using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

public sealed class PreferencesSettingsRepository : ISettingsRepository
{
    private const string WorkMinutesKey = "timer.workMinutes";
    private const string BreakMinutesKey = "timer.breakMinutes";

    public TimerSettings Load() => new()
    {
        WorkMinutes = Preferences.Default.Get(WorkMinutesKey, TimerSettings.DefaultWorkMinutes),
        BreakMinutes = Preferences.Default.Get(BreakMinutesKey, TimerSettings.DefaultBreakMinutes)
    };

    public void Save(TimerSettings settings)
    {
        Preferences.Default.Set(WorkMinutesKey, settings.WorkMinutes);
        Preferences.Default.Set(BreakMinutesKey, settings.BreakMinutes);
    }
}
