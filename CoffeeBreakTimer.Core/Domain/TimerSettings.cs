namespace CoffeeBreakTimer.Core.Domain;

public sealed class TimerSettings
{
    public const int DefaultWorkMinutes = 25;
    public const int DefaultBreakMinutes = 5;
    public const int MinimumMinutes = 1;
    public const int MaximumMinutes = 240;

    private int _workMinutes = DefaultWorkMinutes;
    private int _breakMinutes = DefaultBreakMinutes;

    public int WorkMinutes
    {
        get => _workMinutes;
        set => _workMinutes = ClampMinutes(value);
    }

    public int BreakMinutes
    {
        get => _breakMinutes;
        set => _breakMinutes = ClampMinutes(value);
    }

    public TimeSpan WorkDuration => TimeSpan.FromMinutes(WorkMinutes);

    public TimeSpan BreakDuration => TimeSpan.FromMinutes(BreakMinutes);

    public static TimerSettings Default => new()
    {
        WorkMinutes = DefaultWorkMinutes,
        BreakMinutes = DefaultBreakMinutes
    };

    public TimerSettings Copy() => new()
    {
        WorkMinutes = WorkMinutes,
        BreakMinutes = BreakMinutes
    };

    private static int ClampMinutes(int minutes) =>
        Math.Clamp(minutes, MinimumMinutes, MaximumMinutes);
}
