using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;

namespace CoffeeBreakTimer.App.Services;

public sealed class LocalNotificationService : INotificationService
{
    public Task ShowSessionCompletedAsync(SessionType completedSessionType, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
