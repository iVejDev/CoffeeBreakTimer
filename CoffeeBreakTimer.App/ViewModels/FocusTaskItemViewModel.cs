using CoffeeBreakTimer.Core.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeeBreakTimer.App.ViewModels;

public partial class FocusTaskItemViewModel : ObservableObject
{
    private readonly Func<FocusTaskItemViewModel, Task> _toggleTask;
    private readonly Func<FocusTaskItemViewModel, Task> _deleteTask;
    private readonly Func<FocusTaskItemViewModel, Task> _saveTask;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(CompletionMark))]
    private bool isCompleted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(FocusCardProgressText))]
    [NotifyPropertyChangedFor(nameof(IsEstimateReached))]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private int completedFocusSessions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShortTitle))]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private string title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(FocusCardProgressText))]
    [NotifyPropertyChangedFor(nameof(IsEstimateReached))]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private int? estimatedFocusSessions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))]
    private bool isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveEdit))]
    [NotifyCanExecuteChangedFor(nameof(SaveEditCommand))]
    private string editTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSaveEdit))]
    [NotifyCanExecuteChangedFor(nameof(SaveEditCommand))]
    private string editEstimatedSessionsText = string.Empty;

    public FocusTaskItemViewModel(
        FocusTask task,
        Func<FocusTaskItemViewModel, Task> toggleTask,
        Func<FocusTaskItemViewModel, Task> deleteTask,
        Func<FocusTaskItemViewModel, Task> saveTask)
    {
        _toggleTask = toggleTask;
        _deleteTask = deleteTask;
        _saveTask = saveTask;
        Id = task.Id;
        Title = task.Title;
        EstimatedFocusSessions = task.EstimatedFocusSessions;
        CompletedFocusSessions = task.CompletedFocusSessions;
        CreatedAt = task.CreatedAt;
        CompletedAt = task.CompletedAt;
        IsCompleted = task.IsCompleted;
    }

    public Guid Id { get; }

    public string ShortTitle => Truncate(Title, 44);

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public bool IsNotEditing => !IsEditing;

    public bool CanSaveEdit => !string.IsNullOrWhiteSpace(EditTitle);

    public bool IsEstimateReached =>
        EstimatedFocusSessions is not null &&
        CompletedFocusSessions >= EstimatedFocusSessions.Value;

    public string ProgressText
    {
        get
        {
            if (EstimatedFocusSessions is null)
            {
                return $"{CompletedFocusSessions} focus sessions";
            }

            return $"{CompletedFocusSessions} / {EstimatedFocusSessions} focus sessions";
        }
    }

    public string FocusCardProgressText => IsEstimateReached
        ? $"Goal reached - {ProgressText}"
        : ProgressText;

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

    public bool TryApplyEdit()
    {
        int? estimatedSessions = null;

        if (int.TryParse(EditEstimatedSessionsText, out var parsedEstimate))
        {
            estimatedSessions = parsedEstimate;
        }

        var model = ToModel();

        try
        {
            model.Rename(EditTitle);
            model.SetEstimatedFocusSessions(estimatedSessions);
        }
        catch (ArgumentException)
        {
            return false;
        }

        Title = model.Title;
        EstimatedFocusSessions = model.EstimatedFocusSessions;
        return true;
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

    [RelayCommand]
    private void BeginEdit()
    {
        EditTitle = Title;
        EditEstimatedSessionsText = EstimatedFocusSessions?.ToString() ?? string.Empty;
        IsEditing = true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveEdit))]
    private async Task SaveEditAsync()
    {
        if (!TryApplyEdit())
        {
            return;
        }

        IsEditing = false;
        await _saveTask(this);
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditTitle = Title;
        EditEstimatedSessionsText = EstimatedFocusSessions?.ToString() ?? string.Empty;
        IsEditing = false;
    }
}
