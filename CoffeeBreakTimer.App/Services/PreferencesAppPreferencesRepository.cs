using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

public sealed class PreferencesAppPreferencesRepository : IAppPreferencesRepository
{
    private const string NotificationSoundsEnabledKey = "app.notificationSoundsEnabled";
    private const string RainAmbienceEnabledKey = "app.rainAmbienceEnabled";
    private const string ChillAmbienceEnabledKey = "app.chillAmbienceEnabled";
    private const string AmbienceVolumeKey = "app.ambienceVolume";

    public AppPreferences Load() => new()
    {
        NotificationSoundsEnabled = Preferences.Default.Get(NotificationSoundsEnabledKey, true),
        RainAmbienceEnabled = Preferences.Default.Get(RainAmbienceEnabledKey, false),
        ChillAmbienceEnabled = Preferences.Default.Get(ChillAmbienceEnabledKey, false),
        AmbienceVolume = Preferences.Default.Get(AmbienceVolumeKey, AppPreferences.DefaultAmbienceVolume)
    };

    public void Save(AppPreferences preferences)
    {
        Preferences.Default.Set(NotificationSoundsEnabledKey, preferences.NotificationSoundsEnabled);
        Preferences.Default.Set(RainAmbienceEnabledKey, preferences.RainAmbienceEnabled);
        Preferences.Default.Set(ChillAmbienceEnabledKey, preferences.ChillAmbienceEnabled);
        Preferences.Default.Set(AmbienceVolumeKey, preferences.AmbienceVolume);
    }
}
