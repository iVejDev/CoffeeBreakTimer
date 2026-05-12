namespace CoffeeBreakTimer.Core.Interfaces;

public interface ITimerService
{
    bool IsRunning { get; }

    event EventHandler? Tick;

    void Start(TimeSpan interval);

    void Stop();
}
