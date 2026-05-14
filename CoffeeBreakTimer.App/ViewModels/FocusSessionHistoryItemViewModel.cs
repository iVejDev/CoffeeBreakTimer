using CoffeeBreakTimer.Core.Domain;

namespace CoffeeBreakTimer.App.ViewModels;

public sealed class FocusSessionHistoryItemViewModel
{
    public FocusSessionHistoryItemViewModel(FocusSessionRecord session)
    {
        CompletedAtDisplay = session.CompletedAt.LocalDateTime.ToString("MMM d, HH:mm");
        DurationDisplay = $"{session.FocusMinutes} min";
        TaskTitleDisplay = string.IsNullOrWhiteSpace(session.TaskTitle)
            ? "No linked task"
            : session.TaskTitle;
    }

    public string CompletedAtDisplay { get; }

    public string DurationDisplay { get; }

    public string TaskTitleDisplay { get; }
}
