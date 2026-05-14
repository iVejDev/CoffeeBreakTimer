namespace CoffeeBreakTimer.Core.Domain;

public sealed class FocusSessionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;

    public int FocusMinutes { get; set; }

    public Guid? TaskId { get; set; }

    public string? TaskTitle { get; set; }

    public static FocusSessionRecord Create(TimeSpan duration, Guid? taskId = null, string? taskTitle = null)
    {
        return new FocusSessionRecord
        {
            Id = Guid.NewGuid(),
            CompletedAt = DateTimeOffset.UtcNow,
            FocusMinutes = Math.Max(1, (int)Math.Round(duration.TotalMinutes)),
            TaskId = taskId,
            TaskTitle = NormalizeTaskTitle(taskTitle)
        };
    }

    private static string? NormalizeTaskTitle(string? taskTitle)
    {
        if (string.IsNullOrWhiteSpace(taskTitle))
        {
            return null;
        }

        return taskTitle.Trim();
    }
}
