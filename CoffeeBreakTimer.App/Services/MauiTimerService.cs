using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

public sealed class MauiTimerService : ITimerService
{
    private IDispatcherTimer? _timer;

    public bool IsRunning => _timer?.IsRunning == true;

    public event EventHandler? Tick;

    public void Start(TimeSpan interval)
    {
        Stop();

        _timer = Application.Current?.Dispatcher.CreateTimer()
            ?? throw new InvalidOperationException("The MAUI dispatcher is not available.");

        _timer.Interval = interval;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Stop()
    {
        if (_timer is null)
        {
            return;
        }

        _timer.Tick -= OnTick;
        _timer.Stop();
        _timer = null;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }
}
