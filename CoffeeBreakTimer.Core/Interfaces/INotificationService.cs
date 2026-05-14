using CoffeeBreakTimer.Core.Domain.Enums;

namespace CoffeeBreakTimer.Core.Interfaces;

public interface INotificationService
{
    Task ShowSessionCompletedAsync(SessionType completedSessionType, CancellationToken cancellationToken = default);
}
