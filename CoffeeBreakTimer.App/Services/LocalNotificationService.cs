using CoffeeBreakTimer.Core.Domain.Enums;
using CoffeeBreakTimer.Core.Interfaces;

#if WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
#endif

namespace CoffeeBreakTimer.App.Services;

public sealed class LocalNotificationService : INotificationService
{
    private readonly IUserDialogService _dialogs;
    private bool _isRegistered;

    public LocalNotificationService(IUserDialogService dialogs)
    {
        _dialogs = dialogs;

#if WINDOWS
        TryRegisterWindowsNotifications();
#endif
    }

    public async Task ShowSessionCompletedAsync(SessionType completedSessionType, CancellationToken cancellationToken = default)
    {
        var (title, message) = CreateSessionMessage(completedSessionType);

#if WINDOWS
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (TryShowWindowsNotification(title, message))
        {
            return;
        }
#endif

        await _dialogs.AlertAsync(title, message, "OK", cancellationToken);
    }

    private static (string Title, string Message) CreateSessionMessage(SessionType completedSessionType)
    {
        return completedSessionType == SessionType.Work
            ? ("Coffee break time", "Your focus session is done. Time for a soft pause.")
            : ("Back to focus", "Your break is done. Step gently back into focus.");
    }

#if WINDOWS
    private bool TryShowWindowsNotification(string title, string message)
    {
        try
        {
            TryRegisterWindowsNotifications();

            if (AppNotificationManager.Default.Setting != AppNotificationSetting.Enabled)
            {
                return false;
            }

            var notification = new AppNotificationBuilder()
                .AddText(title)
                .AddText(message)
                .SetAttributionText("CoffeeBreakerTimer")
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
            return true;
        }
        catch
        {
            // Notifications are best-effort. Audio and in-app state still handle session completion.
            return false;
        }
    }

    private void TryRegisterWindowsNotifications()
    {
        if (_isRegistered)
        {
            return;
        }

        try
        {
            AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            AppNotificationManager.Default.Register();
            _isRegistered = true;
        }
        catch
        {
            _isRegistered = false;
        }
    }

    private static void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
    }
#endif
}
