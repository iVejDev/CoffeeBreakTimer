using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;
using System.Diagnostics;

namespace CoffeeBreakTimer.Core.Services;

public sealed class CoffeeTimerService : IDisposable
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(100);

    private readonly ITimerService _timer;
    private readonly IAudioPlayer _audio;
    private readonly Stopwatch _stopwatch = new();

    private TimerSettings _settings = TimerSettings.Default;
    private TimeSpan _elapsedBeforePause = TimeSpan.Zero;
    private bool _disposed;

    public CoffeeTimerService(ITimerService timer, IAudioPlayer audio)
    {
        _timer = timer;
        _audio = audio;

        _timer.Tick += OnTick;

        CurrentSession = SessionType.Work;
        RunState = TimerRunState.Ready;
        CurrentDuration = _settings.WorkDuration;
        CurrentSnapshot = CreateSnapshot(TimeSpan.Zero);
    }

    public event EventHandler<CoffeeTimerSnapshot>? StateChanged;

    public SessionType CurrentSession { get; private set; }

    public TimerRunState RunState { get; private set; }

    public TimeSpan CurrentDuration { get; private set; }

    public CoffeeTimerSnapshot CurrentSnapshot { get; private set; }

    public void UpdateSettings(TimerSettings settings)
    {
        _settings = settings.Copy();

        if (RunState is TimerRunState.Ready or TimerRunState.Completed)
        {
            CurrentSession = SessionType.Work;
            CurrentDuration = _settings.WorkDuration;
            PublishSnapshot(TimeSpan.Zero);
        }
    }

    public void StartWorkSession(TimerSettings settings)
    {
        UpdateSettings(settings);
        StartSession(SessionType.Work, _settings.WorkDuration);
    }

    public void Pause()
    {
        if (RunState != TimerRunState.Running)
        {
            return;
        }

        _elapsedBeforePause = GetElapsed();
        _stopwatch.Stop();
        _timer.Stop();
        RunState = TimerRunState.Paused;
        PublishSnapshot(_elapsedBeforePause);
    }

    public void Resume()
    {
        if (RunState != TimerRunState.Paused)
        {
            return;
        }

        _stopwatch.Restart();
        RunState = TimerRunState.Running;
        _timer.Start(TickInterval);
        PublishSnapshot(GetElapsed());
    }

    public void Reset()
    {
        _timer.Stop();
        _stopwatch.Reset();
        _elapsedBeforePause = TimeSpan.Zero;
        CurrentSession = SessionType.Work;
        CurrentDuration = _settings.WorkDuration;
        RunState = TimerRunState.Ready;
        PublishSnapshot(TimeSpan.Zero);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _timer.Tick -= OnTick;
        _timer.Stop();
        _disposed = true;
    }

    private void StartSession(SessionType sessionType, TimeSpan duration)
    {
        _timer.Stop();
        _stopwatch.Restart();
        _elapsedBeforePause = TimeSpan.Zero;
        CurrentSession = sessionType;
        CurrentDuration = duration;
        RunState = TimerRunState.Running;

        _timer.Start(TickInterval);
        PublishSnapshot(TimeSpan.Zero);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var elapsed = GetElapsed();

        if (elapsed >= CurrentDuration)
        {
            CompleteCurrentSession();
            return;
        }

        PublishSnapshot(elapsed);
    }

    private void CompleteCurrentSession()
    {
        _timer.Stop();
        _stopwatch.Stop();
        _elapsedBeforePause = CurrentDuration;
        RunState = TimerRunState.Completed;
        PublishSnapshot(CurrentDuration);
        _audio.Play("session_end");

        if (CurrentSession == SessionType.Work)
        {
            StartSession(SessionType.Break, _settings.BreakDuration);
            return;
        }

        CurrentSession = SessionType.Work;
        CurrentDuration = _settings.WorkDuration;
        _elapsedBeforePause = TimeSpan.Zero;
        RunState = TimerRunState.Ready;
        PublishSnapshot(TimeSpan.Zero);
    }

    private TimeSpan GetElapsed()
    {
        var elapsed = _elapsedBeforePause;

        if (RunState == TimerRunState.Running)
        {
            elapsed += _stopwatch.Elapsed;
        }

        return elapsed > CurrentDuration ? CurrentDuration : elapsed;
    }

    private void PublishSnapshot(TimeSpan elapsed)
    {
        CurrentSnapshot = CreateSnapshot(elapsed);
        StateChanged?.Invoke(this, CurrentSnapshot);
    }

    private CoffeeTimerSnapshot CreateSnapshot(TimeSpan elapsed)
    {
        var durationSeconds = Math.Max(CurrentDuration.TotalSeconds, 1);
        var progress = Math.Clamp(elapsed.TotalSeconds / durationSeconds, 0.0, 1.0);
        var remaining = CurrentDuration - elapsed;

        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        var coffeeLevel = CurrentSession == SessionType.Work
            ? new CoffeeLevel(1.0 - progress)
            : new CoffeeLevel(progress);

        return new CoffeeTimerSnapshot(
            CurrentSession,
            RunState,
            CurrentDuration,
            remaining,
            progress,
            coffeeLevel);
    }
}
