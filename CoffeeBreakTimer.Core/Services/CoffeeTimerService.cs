
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

    public SessionType CurrentSession { get; private set; }
    public CoffeeLevel Level { get; private set; }


    public CoffeeTimerService(ITimerService timer, ISettingsRepository settings, IAudioPlayer audio)
    {
        _timer = timer;
        _settings = settings;
        _audio = audio;

        CurrentSession = SessionType.Work;
        Level = new CoffeeLevel(1.0);
    }

}