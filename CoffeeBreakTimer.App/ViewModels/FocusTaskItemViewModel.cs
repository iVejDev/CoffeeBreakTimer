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

    public int? EstimatedFocusSessions { get; }

    public int CompletedFocusSessions { get; }

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
