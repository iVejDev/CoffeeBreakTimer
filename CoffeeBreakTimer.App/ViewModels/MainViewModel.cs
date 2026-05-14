using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;
using CoffeeBreakTimer.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CoffeeBreakTimer.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly CoffeeTimerService _timerService;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAppPreferencesRepository _appPreferencesRepository;
    private readonly IAudioPlayer _audioPlayer;
    private readonly IAmbiencePlayer _ambiencePlayer;
    private readonly ITaskRepository _taskRepository;
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly CancellationTokenSource _quoteRotationTokenSource = new();
    private readonly List<FocusSessionRecord> _focusSessionRecords = [];
    private bool _isLoadingSettings;
    private bool _isLoadingAppPreferences;
    private bool _isUpdatingDurationText;
    private bool _focusCompletionRecorded;
    private bool _disposed;
    private int _quoteIndex;
    private QuoteMode _quoteMode = QuoteMode.Focus;

    private static readonly string[] FocusQuotes =
    [
        "Small steps. Deep focus.",
        "Let the next minute be enough.",
        "Quiet effort compounds.",
        "Stay with one good task.",
        "Make room for the work."
    ];

    private static readonly string[] RestQuotes =
    [
        "Breathe out. You earned this.",
        "Let the coffee come back.",
        "A soft pause is productive too.",
        "Rest is part of the rhythm.",
        "Return gently, not urgently."
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SessionTitle))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private SessionType sessionType = SessionType.Work;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StartButtonText))]
    [NotifyPropertyChangedFor(nameof(IsDurationEditingEnabled))]
    private TimerRunState runState = TimerRunState.Ready;

    [ObservableProperty]
    private string timeDisplay = "25:00";

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private double coffeeLevel = 1.0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WorkMinutesDisplay))]
    private double workMinutes = TimerSettings.DefaultWorkMinutes;

    [ObservableProperty]
    private string workHoursText = "0";

    [ObservableProperty]
    private string workRemainingMinutesText = TimerSettings.DefaultWorkMinutes.ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BreakMinutesDisplay))]
    private double breakMinutes = TimerSettings.DefaultBreakMinutes;

    [ObservableProperty]
    private string breakHoursText = "0";

    [ObservableProperty]
    private string breakRemainingMinutesText = TimerSettings.DefaultBreakMinutes.ToString();

    [ObservableProperty]
    private string quoteText = FocusQuotes[0];

    [ObservableProperty]
    private bool isRainAmbienceEnabled;

    [ObservableProperty]
    private bool isChillAmbienceEnabled;

    [ObservableProperty]
    private bool notificationSoundsEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmbienceVolumeDisplay))]
    private double ambienceVolume = 0.55;

    [ObservableProperty]
    private WorkspaceSection selectedWorkspaceSection = WorkspaceSection.Focus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTaskCommand))]
    private string newTaskTitle = string.Empty;

    [ObservableProperty]
    private string newTaskEstimatedSessionsText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFocusTaskText))]
    private FocusTaskItemViewModel? selectedFocusTask;

    public MainViewModel(
        CoffeeTimerService timerService,
        ISettingsRepository settingsRepository,
        IAppPreferencesRepository appPreferencesRepository,
        IAudioPlayer audioPlayer,
        IAmbiencePlayer ambiencePlayer,
        ITaskRepository taskRepository,
        IStatisticsRepository statisticsRepository)
    {
        _timerService = timerService;
        _settingsRepository = settingsRepository;
        _appPreferencesRepository = appPreferencesRepository;
        _audioPlayer = audioPlayer;
        _ambiencePlayer = ambiencePlayer;
        _taskRepository = taskRepository;
        _statisticsRepository = statisticsRepository;

        _timerService.StateChanged += OnTimerStateChanged;
        LoadSettings();
        LoadAppPreferences();
        ApplySnapshot(_timerService.CurrentSnapshot);
        _ = LoadTasksAsync();
        _ = LoadStatisticsAsync();
        _ = RotateQuotesAsync(_quoteRotationTokenSource.Token);
    }

    public string SessionTitle => SessionType == SessionType.Work ? "Focus" : "Break";

    public string StatusText => RunState switch
    {
        TimerRunState.Running when SessionType == SessionType.Work => "Coffee draining while you focus",
        TimerRunState.Running => "Coffee refilling while you recover",
        TimerRunState.Paused => "Paused",
        TimerRunState.Completed => "Session complete",
        _ => "Ready"
    };

    public string WorkMinutesDisplay => $"{Math.Round(WorkMinutes):0} min";

    public string BreakMinutesDisplay => $"{Math.Round(BreakMinutes):0} min";

    public string StartButtonText => RunState == TimerRunState.Paused ? "Resume" : "Start";

    public bool IsDurationEditingEnabled => RunState is TimerRunState.Ready or TimerRunState.Completed;

    public bool CanStart => RunState != TimerRunState.Running;

    public bool CanPause => RunState == TimerRunState.Running;

    public bool CanReset => RunState != TimerRunState.Ready || SessionType != SessionType.Work || CoffeeLevel < 1.0;

    public string AmbienceVolumeDisplay => $"{Math.Round(AmbienceVolume * 100):0}%";

    public ObservableCollection<FocusTaskItemViewModel> Tasks { get; } = [];

    public ObservableCollection<FocusTaskItemViewModel> ActiveTasks { get; } = [];

    public bool HasTasks => Tasks.Count > 0;

    public bool HasNoTasks => !HasTasks;

    public bool CanAddTask => !string.IsNullOrWhiteSpace(NewTaskTitle);

    public bool HasActiveTasks => ActiveTasks.Count > 0;

    public string SelectedFocusTaskText => SelectedFocusTask is null
        ? "No task selected"
        : SelectedFocusTask.DisplayText;

    public string StatisticsFocusTimeTodayDisplay
    {
        get
        {
            var totalMinutes = _focusSessionRecords
                .Where(session => IsToday(session.CompletedAt))
                .Sum(session => session.FocusMinutes);

            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        }
    }

    public string StatisticsCompletedSessionsDisplay => _focusSessionRecords.Count.ToString();

    public string StatisticsCompletedTasksDisplay => Tasks.Count(task => task.IsCompleted).ToString();

    public string StatisticsCurrentStreakDisplay => CalculateCurrentStreak().ToString();

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        var settings = CreateSettings();
        _settingsRepository.Save(settings);

        if (RunState == TimerRunState.Paused)
        {
            _timerService.Resume();
            return;
        }

        _timerService.StartWorkSession(settings);
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private void Pause()
    {
        _timerService.Pause();
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        _timerService.Reset();
    }

    [RelayCommand]
    private void SelectWorkspaceSection(string section)
    {
        if (Enum.TryParse<WorkspaceSection>(section, ignoreCase: true, out var selectedSection))
        {
            SelectedWorkspaceSection = selectedSection;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddTask))]
    private async Task AddTaskAsync()
    {
        int? estimatedSessions = null;

        if (int.TryParse(NewTaskEstimatedSessionsText, out var parsedEstimate))
        {
            estimatedSessions = parsedEstimate;
        }

        FocusTask task;

        try
        {
            task = FocusTask.Create(NewTaskTitle, estimatedSessions);
        }
        catch (ArgumentException)
        {
            return;
        }

        var taskItem = CreateTaskItem(task);
        Tasks.Insert(0, taskItem);
        SelectedFocusTask ??= taskItem;
        NewTaskTitle = string.Empty;
        NewTaskEstimatedSessionsText = string.Empty;
        RefreshTaskState();
        await SaveTasksAsync();
    }

    private async Task ToggleTaskAsync(FocusTaskItemViewModel task)
    {
        task.IsCompleted = !task.IsCompleted;
        RefreshTaskState();
        await SaveTasksAsync();
    }

    private async Task DeleteTaskAsync(FocusTaskItemViewModel task)
    {
        Tasks.Remove(task);
        if (SelectedFocusTask == task)
        {
            SelectedFocusTask = null;
        }

        RefreshTaskState();
        await SaveTasksAsync();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timerService.StateChanged -= OnTimerStateChanged;
        _ambiencePlayer.StopAll();
        _quoteRotationTokenSource.Cancel();
        _quoteRotationTokenSource.Dispose();
        _disposed = true;
    }

    partial void OnWorkMinutesChanged(double value)
    {
        UpdateDurationParts(value, isWorkDuration: true);
        PersistSettings();
    }

    partial void OnBreakMinutesChanged(double value)
    {
        UpdateDurationParts(value, isWorkDuration: false);
        PersistSettings();
    }

    partial void OnWorkHoursTextChanged(string value)
    {
        ApplyTypedDurationParts(value, WorkRemainingMinutesText, minutes => WorkMinutes = minutes);
    }

    partial void OnWorkRemainingMinutesTextChanged(string value)
    {
        ApplyTypedDurationParts(WorkHoursText, value, minutes => WorkMinutes = minutes);
    }

    partial void OnBreakHoursTextChanged(string value)
    {
        ApplyTypedDurationParts(value, BreakRemainingMinutesText, minutes => BreakMinutes = minutes);
    }

    partial void OnBreakRemainingMinutesTextChanged(string value)
    {
        ApplyTypedDurationParts(BreakHoursText, value, minutes => BreakMinutes = minutes);
    }

    partial void OnRunStateChanged(TimerRunState value)
    {
        RefreshCommandState();
    }

    partial void OnSessionTypeChanged(SessionType value)
    {
        RefreshCommandState();
    }

    partial void OnCoffeeLevelChanged(double value)
    {
        RefreshCommandState();
    }

    partial void OnSelectedFocusTaskChanged(FocusTaskItemViewModel? value)
    {
        OnPropertyChanged(nameof(SelectedFocusTaskText));
    }

    partial void OnIsRainAmbienceEnabledChanged(bool value)
    {
        _ambiencePlayer.SetEnabled(AmbienceTrack.Rain, value);
        PersistAppPreferences();
    }

    partial void OnIsChillAmbienceEnabledChanged(bool value)
    {
        _ambiencePlayer.SetEnabled(AmbienceTrack.Chill, value);
        PersistAppPreferences();
    }

    partial void OnNotificationSoundsEnabledChanged(bool value)
    {
        _audioPlayer.IsEnabled = value;
        PersistAppPreferences();
    }

    partial void OnAmbienceVolumeChanged(double value)
    {
        _ambiencePlayer.SetVolume(value);
        PersistAppPreferences();
    }

    private void LoadSettings()
    {
        _isLoadingSettings = true;

        var settings = _settingsRepository.Load();
        WorkMinutes = settings.WorkMinutes;
        BreakMinutes = settings.BreakMinutes;
        _timerService.UpdateSettings(settings);

        _isLoadingSettings = false;
    }

    private void LoadAppPreferences()
    {
        _isLoadingAppPreferences = true;

        var preferences = _appPreferencesRepository.Load();
        NotificationSoundsEnabled = preferences.NotificationSoundsEnabled;
        AmbienceVolume = preferences.AmbienceVolume;
        IsRainAmbienceEnabled = preferences.RainAmbienceEnabled;
        IsChillAmbienceEnabled = preferences.ChillAmbienceEnabled;

        _audioPlayer.IsEnabled = NotificationSoundsEnabled;
        _ambiencePlayer.SetVolume(AmbienceVolume);
        _ambiencePlayer.SetEnabled(AmbienceTrack.Rain, IsRainAmbienceEnabled);
        _ambiencePlayer.SetEnabled(AmbienceTrack.Chill, IsChillAmbienceEnabled);

        _isLoadingAppPreferences = false;
    }

    private void PersistSettings()
    {
        if (_isLoadingSettings)
        {
            return;
        }

        var settings = CreateSettings();
        _settingsRepository.Save(settings);
        _timerService.UpdateSettings(settings);
    }

    private void PersistAppPreferences()
    {
        if (_isLoadingAppPreferences)
        {
            return;
        }

        _appPreferencesRepository.Save(new AppPreferences
        {
            NotificationSoundsEnabled = NotificationSoundsEnabled,
            RainAmbienceEnabled = IsRainAmbienceEnabled,
            ChillAmbienceEnabled = IsChillAmbienceEnabled,
            AmbienceVolume = AmbienceVolume
        });
    }

    private TimerSettings CreateSettings() => new()
    {
        WorkMinutes = (int)Math.Round(WorkMinutes),
        BreakMinutes = (int)Math.Round(BreakMinutes)
    };

    private void OnTimerStateChanged(object? sender, CoffeeTimerSnapshot snapshot)
    {
        MainThread.BeginInvokeOnMainThread(() => ApplySnapshot(snapshot));
    }

    private void ApplySnapshot(CoffeeTimerSnapshot snapshot)
    {
        if (snapshot.SessionType == SessionType.Work && snapshot.RunState == TimerRunState.Running)
        {
            _focusCompletionRecorded = false;
        }

        if (snapshot.SessionType == SessionType.Work &&
            snapshot.RunState == TimerRunState.Completed &&
            !_focusCompletionRecorded)
        {
            _focusCompletionRecorded = true;
            _ = RegisterCompletedFocusSessionAsync(snapshot.Duration);
        }

        SessionType = snapshot.SessionType;
        RunState = snapshot.RunState;
        Progress = snapshot.Progress;
        CoffeeLevel = snapshot.CoffeeLevel.Value;
        TimeDisplay = FormatTime(snapshot.Remaining);
        UpdateQuoteMode(snapshot.SessionType == SessionType.Break ? QuoteMode.Rest : QuoteMode.Focus);
    }

    private void RefreshCommandState()
    {
        StartCommand.NotifyCanExecuteChanged();
        PauseCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanPause));
        OnPropertyChanged(nameof(CanReset));
    }

    private async Task LoadTasksAsync()
    {
        var tasks = await _taskRepository.LoadAsync();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Tasks.Clear();

            foreach (var task in tasks)
            {
                Tasks.Add(CreateTaskItem(task));
            }

            RefreshTaskState();
            RefreshStatisticsState();
        });
    }

    private async Task LoadStatisticsAsync()
    {
        var sessions = await _statisticsRepository.LoadFocusSessionsAsync();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _focusSessionRecords.Clear();
            _focusSessionRecords.AddRange(sessions);
            RefreshStatisticsState();
        });
    }

    private async Task SaveTasksAsync()
    {
        var tasks = Tasks.Select(task => task.ToModel()).ToList();
        await _taskRepository.SaveAsync(tasks);
    }

    private FocusTaskItemViewModel CreateTaskItem(FocusTask task)
    {
        return new FocusTaskItemViewModel(task, ToggleTaskAsync, DeleteTaskAsync);
    }

    private void RefreshTaskState()
    {
        RefreshActiveTasks();
        OnPropertyChanged(nameof(HasTasks));
        OnPropertyChanged(nameof(HasNoTasks));
        OnPropertyChanged(nameof(HasActiveTasks));
        OnPropertyChanged(nameof(SelectedFocusTaskText));
        RefreshStatisticsState();
    }

    private async Task RegisterCompletedFocusSessionAsync(TimeSpan focusDuration)
    {
        _focusSessionRecords.Add(FocusSessionRecord.Create(focusDuration));

        if (SelectedFocusTask is not null && !SelectedFocusTask.IsCompleted)
        {
            SelectedFocusTask.RegisterCompletedFocusSession();
            OnPropertyChanged(nameof(SelectedFocusTaskText));
            await SaveTasksAsync();
        }

        await _statisticsRepository.SaveFocusSessionsAsync(_focusSessionRecords);
        RefreshStatisticsState();
    }

    private void RefreshActiveTasks()
    {
        var selectedTaskId = SelectedFocusTask?.Id;
        var activeTasks = Tasks.Where(task => !task.IsCompleted).ToList();

        ActiveTasks.Clear();

        foreach (var task in activeTasks)
        {
            ActiveTasks.Add(task);
        }

        if (selectedTaskId is null)
        {
            return;
        }

        SelectedFocusTask = ActiveTasks.FirstOrDefault(task => task.Id == selectedTaskId);
    }

    private void RefreshStatisticsState()
    {
        OnPropertyChanged(nameof(StatisticsFocusTimeTodayDisplay));
        OnPropertyChanged(nameof(StatisticsCompletedSessionsDisplay));
        OnPropertyChanged(nameof(StatisticsCompletedTasksDisplay));
        OnPropertyChanged(nameof(StatisticsCurrentStreakDisplay));
    }

    private int CalculateCurrentStreak()
    {
        var sessionDates = _focusSessionRecords
            .Select(session => DateOnly.FromDateTime(session.CompletedAt.LocalDateTime.Date))
            .Distinct()
            .ToHashSet();

        var cursor = DateOnly.FromDateTime(DateTime.Now.Date);
        var streak = 0;

        while (sessionDates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private static bool IsToday(DateTimeOffset completedAt)
    {
        return completedAt.LocalDateTime.Date == DateTime.Now.Date;
    }

    private static string FormatTime(TimeSpan time)
    {
        var totalMinutes = (int)time.TotalMinutes;
        return $"{totalMinutes:00}:{time.Seconds:00}";
    }

    private void ApplyTypedDurationParts(string hoursText, string minutesText, Action<double> update)
    {
        if (_isUpdatingDurationText)
        {
            return;
        }

        var hours = ParseDurationPart(hoursText);
        var minutes = ParseDurationPart(minutesText);

        if (hours is null && minutes is null)
        {
            return;
        }

        update(NormalizeMinutes(((hours ?? 0) * 60) + (minutes ?? 0)));
    }

    private void UpdateDurationParts(double totalMinutes, bool isWorkDuration)
    {
        var normalizedMinutes = (int)NormalizeMinutes(totalMinutes);
        var hours = normalizedMinutes / 60;
        var minutes = normalizedMinutes % 60;

        _isUpdatingDurationText = true;

        if (isWorkDuration)
        {
            WorkHoursText = hours.ToString();
            WorkRemainingMinutesText = minutes.ToString();
        }
        else
        {
            BreakHoursText = hours.ToString();
            BreakRemainingMinutesText = minutes.ToString();
        }

        _isUpdatingDurationText = false;
    }

    private static int? ParseDurationPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, out var parsed) ? Math.Max(0, parsed) : null;
    }

    private static double NormalizeMinutes(double minutes)
    {
        return Math.Clamp(Math.Round(minutes), 1, 240);
    }

    private async Task RotateQuotesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(ShowNextQuote);
        }
    }

    private void UpdateQuoteMode(QuoteMode quoteMode)
    {
        if (_quoteMode == quoteMode)
        {
            return;
        }

        _quoteMode = quoteMode;
        _quoteIndex = 0;
        QuoteText = CurrentQuotes[0];
    }

    private void ShowNextQuote()
    {
        var quotes = CurrentQuotes;
        _quoteIndex = (_quoteIndex + 1) % quotes.Length;
        QuoteText = quotes[_quoteIndex];
    }

    private string[] CurrentQuotes => _quoteMode == QuoteMode.Rest ? RestQuotes : FocusQuotes;

    private enum QuoteMode
    {
        Focus,
        Rest
    }
}

public enum WorkspaceSection
{
    Focus,
    Tasks,
    Statistics,
    Settings
}
