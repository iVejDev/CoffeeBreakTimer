namespace CoffeeBreakTimer.Core.Domain;

public sealed class AppPreferences
{
    public const double DefaultAmbienceVolume = 0.55;

    public bool NotificationSoundsEnabled { get; set; } = true;

    public bool RainAmbienceEnabled { get; set; }

    public bool ChillAmbienceEnabled { get; set; }

    private double _ambienceVolume = DefaultAmbienceVolume;

    public double AmbienceVolume
    {
        get => _ambienceVolume;
        set => _ambienceVolume = Math.Clamp(value, 0, 1);
    }

    public static AppPreferences Default => new();
}
