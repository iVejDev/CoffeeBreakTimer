using CoffeeBreakTimer.Core.Domain;

namespace CoffeeBreakTimer.Core.Interfaces;

public interface ISettingsRepository
{
    TimerSettings Load();

    void Save(TimerSettings settings);
}
