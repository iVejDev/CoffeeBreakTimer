using CoffeeBreakTimer.Core.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeeBreakTimer.App.ViewModels;

public partial class FocusTaskItemViewModel : ObservableObject
{
    private readonly Func<FocusTaskItemViewModel, Task> _toggleTask;
    private readonly Func<FocusTaskItemViewModel, Task> _deleteTask;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(CompletionMark))]
    private bool isCompleted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private int completedFocusSessions;

    public FocusTaskItemViewModel(
        FocusTask task,
        Func<FocusTaskItemViewModel, Task> toggleTask,
        Func<FocusTaskItemViewModel, Task> deleteTask)
    {
        _toggleTask = toggleTask;
        _deleteTask = deleteTask;
        Id = task.Id;
        Title = task.Title;
        EstimatedFocusSessions = task.EstimatedFocusSessions;
        CompletedFocusSessions = task.CompletedFocusSessions;
        CreatedAt = task.CreatedAt;
        CompletedAt = task.CompletedAt;
        IsCompleted = task.IsCompleted;
    }

    public Guid Id { get; }

    public string Title { get; }

    public string ShortTitle => Truncate(Title, 44);

    public int? EstimatedFocusSessions { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string ProgressText
    {
        get
        {
            if (EstimatedFocusSessions is null)
            {
                return $"{CompletedFocusSessions} sessions";
            }

            return $"{CompletedFocusSessions}/{EstimatedFocusSessions} sessions";
        }
    }

    public string CompletionMark => IsCompleted ? "✓" : string.Empty;

    public string DisplayText => $"{ShortTitle} - {ProgressText}";

    partial void OnIsCompletedChanged(bool value)
    {
        CompletedAt = value ? DateTimeOffset.UtcNow : null;
    }

    public FocusTask ToModel()
    {
        return new FocusTask
        {
            Id = Id,
            Title = Title,
            IsCompleted = IsCompleted,
            EstimatedFocusSessions = EstimatedFocusSessions,
            CompletedFocusSessions = CompletedFocusSessions,
            CreatedAt = CreatedAt,
            CompletedAt = CompletedAt
        };
    }

    public void RegisterCompletedFocusSession()
    {
        CompletedFocusSessions++;
        OnPropertyChanged(nameof(DisplayText));
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..Math.Max(0, maxLength - 1)]}...";
    }

    [RelayCommand]
    private Task ToggleAsync()
    {
        return _toggleTask(this);
    }

    [RelayCommand]
    private Task DeleteAsync()
    {
        return _deleteTask(this);
    }
}
