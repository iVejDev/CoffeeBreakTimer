using CoffeeBreakTimer.Core.Domain.Enums;

namespace CoffeeBreakTimer.Core.Domain;

public sealed record CoffeeTimerSnapshot(
    SessionType SessionType,
    TimerRunState RunState,
    TimeSpan Duration,
    TimeSpan Remaining,
    double Progress,
    CoffeeLevel CoffeeLevel)
{
    public bool IsRunning => RunState == TimerRunState.Running;
}
