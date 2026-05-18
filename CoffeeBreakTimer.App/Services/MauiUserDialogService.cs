namespace CoffeeBreakTimer.App.Services;

public sealed class MauiUserDialogService : IUserDialogService
{
    public Task AlertAsync(
        string title,
        string message,
        string cancel,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = GetCurrentPage();

            if (page is null)
            {
                return;
            }

            await page.DisplayAlert(title, message, cancel);
        });
    }

    public Task<bool> ConfirmAsync(
        string title,
        string message,
        string accept,
        string cancel,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(false);
        }

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = GetCurrentPage();

            if (page is null)
            {
                return false;
            }

            return await page.DisplayAlert(title, message, accept, cancel);
        });
    }

    private static Page? GetCurrentPage()
    {
        return Application.Current?.Windows
            .FirstOrDefault(window => window.Page is not null)
            ?.Page;
    }
}
