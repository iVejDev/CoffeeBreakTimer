namespace CoffeeBreakTimer.Core.Domain;

public sealed class FocusSessionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;

    public int FocusMinutes { get; set; }

    public static FocusSessionRecord Create(TimeSpan duration)
    {
        return new FocusSessionRecord
        {
            Id = Guid.NewGuid(),
            CompletedAt = DateTimeOffset.UtcNow,
            FocusMinutes = Math.Max(1, (int)Math.Round(duration.TotalMinutes))
        };
    }
}
