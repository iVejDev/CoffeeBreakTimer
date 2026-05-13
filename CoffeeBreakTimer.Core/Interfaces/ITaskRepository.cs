using CoffeeBreakTimer.Core.Domain;

namespace CoffeeBreakTimer.Core.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyList<FocusTask>> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(IReadOnlyCollection<FocusTask> tasks, CancellationToken cancellationToken = default);
}
