using CoffeeBreakTimer.Core.Domain;

namespace CoffeeBreakTimer.Core.Interfaces;

/*  
 Interface for a settings repository that can load and save timer settings.
*/
public interface ISettingsRepository
{
    TimerSettings Load();
    void Save(TimerSettings settings);
}
