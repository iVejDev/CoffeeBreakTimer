namespace CoffeeBreakTimer.Core.Domain;

public sealed class FocusTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int? EstimatedFocusSessions { get; set; }

    public int CompletedFocusSessions { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public static FocusTask Create(string title, int? estimatedFocusSessions = null)
    {
        var normalizedTitle = NormalizeTitle(title);

        return new FocusTask
        {
            Id = Guid.NewGuid(),
            Title = normalizedTitle,
            EstimatedFocusSessions = NormalizeEstimatedSessions(estimatedFocusSessions),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Rename(string title)
    {
        Title = NormalizeTitle(title);
    }

    public void SetEstimatedFocusSessions(int? estimatedFocusSessions)
    {
        EstimatedFocusSessions = NormalizeEstimatedSessions(estimatedFocusSessions);
    }

    public void MarkCompleted()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkActive()
    {
        IsCompleted = false;
        CompletedAt = null;
    }

    public void RegisterCompletedFocusSession()
    {
        CompletedFocusSessions++;
    }

    private static string NormalizeTitle(string title)
    {
        var normalizedTitle = title.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        }

        return normalizedTitle;
    }

    private static int? NormalizeEstimatedSessions(int? estimatedFocusSessions)
    {
        if (estimatedFocusSessions is null)
        {
            return null;
        }

        return Math.Clamp(estimatedFocusSessions.Value, 1, 99);
    }
}
