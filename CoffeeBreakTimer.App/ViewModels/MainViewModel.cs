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
    private readonly IAmbiencePlayer _ambiencePlayer;
    private readonly CancellationTokenSource _quoteRotationTokenSource = new();
    private bool _isLoadingSettings;
    private bool _isUpdatingDurationText;
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
    [NotifyPropertyChangedFor(nameof(AmbienceVolumeDisplay))]
    private double ambienceVolume = 0.55;

    [ObservableProperty]
    private WorkspaceSection selectedWorkspaceSection = WorkspaceSection.Focus;

    public MainViewModel(
        CoffeeTimerService timerService,
        ISettingsRepository settingsRepository,
        IAmbiencePlayer ambiencePlayer)
    {
        _timerService = timerService;
        _settingsRepository = settingsRepository;
        _ambiencePlayer = ambiencePlayer;

        _timerService.StateChanged += OnTimerStateChanged;
        _ambiencePlayer.SetVolume(AmbienceVolume);
        LoadSettings();
        ApplySnapshot(_timerService.CurrentSnapshot);
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

    partial void OnIsRainAmbienceEnabledChanged(bool value)
    {
        _ambiencePlayer.SetEnabled(AmbienceTrack.Rain, value);
    }

    partial void OnIsChillAmbienceEnabledChanged(bool value)
    {
        _ambiencePlayer.SetEnabled(AmbienceTrack.Chill, value);
    }

    partial void OnAmbienceVolumeChanged(double value)
    {
        _ambiencePlayer.SetVolume(value);
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
