using CoffeeBreakTimer.Core.Domain;

namespace CoffeeBreakTimer.Core.Interfaces;

public interface IAppPreferencesRepository
{
    AppPreferences Load();

    void Save(AppPreferences preferences);
}
