
namespace CoffeeBreakTimer.Core.Interfaces;


/*  Interface for a timer service that can start, pause, and stop a timer.
    It also provides events for tick updates and completion notification.
*/
public interface ITimerService
{
    void Start(TimeSpan duration);
    void Pause();
    void Stop();

    event Action<TimeSpan> Tick;
    event Action Completed;
}
