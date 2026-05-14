using CoffeeBreakTimer.Core.Domain;

namespace CoffeeBreakTimer.Core.Interfaces;

public interface IStatisticsRepository
{
    Task<IReadOnlyList<FocusSessionRecord>> LoadFocusSessionsAsync(CancellationToken cancellationToken = default);

    Task SaveFocusSessionsAsync(IReadOnlyCollection<FocusSessionRecord> focusSessions, CancellationToken cancellationToken = default);
}
