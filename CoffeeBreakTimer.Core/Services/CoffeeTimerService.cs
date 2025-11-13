
using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.Core.Services;

/*  
 Service for managing coffee timer sessions and levels.
*/
public class CoffeeTimerService
{
    private readonly ITimerService _timer;
    private readonly ISettingsRepository _settings;
    private readonly IAudioPlayer _audio;
    private TimeSpan _currentDuration;

    public SessionType CurrentSession { get; private set; }
    public CoffeeLevel Level { get; private set; }


    public CoffeeTimerService(ITimerService timer, ISettingsRepository settings, IAudioPlayer audio)
    {
        _timer = timer;
        _settings = settings;
        _audio = audio;

        _timer.Tick += OnTick;
        _timer.Completed += OnCompleted;

        CurrentSession = SessionType.Work;
        Level = new CoffeeLevel(1.0);
    }

    private void OnTick(TimeSpan remaining)
    {
        var ratio = 1.0 - (remaining.TotalSeconds / _currentDuration.TotalSeconds);
        Level = new CoffeeLevel(ratio);
    }

    private void OnCompleted()
    {
        CurrentSession = CurrentSession == SessionType.Work
            ? SessionType.Break
            : SessionType.Work;

        _audio.Play("session_end");
    }

    public void StartSession(TimeSpan duration)
    {
        _currentDuration = duration;
        _timer.Start(duration);
    }

}