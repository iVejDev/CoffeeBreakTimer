using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;
using CoffeeBreakTimer.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CoffeeBreakTimer.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly CoffeeTimerService _timerService;
    private readonly ISettingsRepository _settingsRepository;
    private bool _isLoadingSettings;
    private bool _disposed;

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
    [NotifyPropertyChangedFor(nameof(BreakMinutesDisplay))]
    private double breakMinutes = TimerSettings.DefaultBreakMinutes;

    public MainViewModel(CoffeeTimerService timerService, ISettingsRepository settingsRepository)
    {
        _timerService = timerService;
        _settingsRepository = settingsRepository;

        _timerService.StateChanged += OnTimerStateChanged;
        LoadSettings();
        ApplySnapshot(_timerService.CurrentSnapshot);
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timerService.StateChanged -= OnTimerStateChanged;
        _disposed = true;
    }

    partial void OnWorkMinutesChanged(double value)
    {
        PersistSettings();
    }

    partial void OnBreakMinutesChanged(double value)
    {
        PersistSettings();
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

    private void LoadSettings()
    {
        _isLoadingSettings = true;

        var settings = _settingsRepository.Load();
        WorkMinutes = settings.WorkMinutes;
        BreakMinutes = settings.BreakMinutes;
        _timerService.UpdateSettings(settings);

        _isLoadingSettings = false;
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
        SessionType = snapshot.SessionType;
        RunState = snapshot.RunState;
        Progress = snapshot.Progress;
        CoffeeLevel = snapshot.CoffeeLevel.Value;
        TimeDisplay = FormatTime(snapshot.Remaining);
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

    private static string FormatTime(TimeSpan time)
    {
        var totalMinutes = (int)time.TotalMinutes;
        return $"{totalMinutes:00}:{time.Seconds:00}";
    }
}
