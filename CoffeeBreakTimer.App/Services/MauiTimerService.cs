using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

public class MauiTimerService : ITimerService
{
    private IDispatcherTimer? _timer;
    private TimeSpan _remaining;

    public event Action<TimeSpan>? Tick;
    public event Action? Completed;

    public void Start(TimeSpan duration)
    {
        _remaining = duration;

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Pause()
    {
        _timer?.Stop();
    }

    public void Stop()
    {
        _timer?.Stop();
        _remaining = TimeSpan.Zero;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _remaining -= TimeSpan.FromSeconds(1);

        Tick?.Invoke(_remaining);

        if (_remaining <= TimeSpan.Zero)
        {
            _timer?.Stop();
            Completed?.Invoke();
        }
    }
}
