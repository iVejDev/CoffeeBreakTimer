namespace CoffeeBreakTimer.App.Services;

public sealed class MauiUserDialogService : IUserDialogService
{
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
            var page = Application.Current?.Windows
                .FirstOrDefault(window => window.Page is not null)
                ?.Page;

            if (page is null)
            {
                return false;
            }

            return await page.DisplayAlert(title, message, accept, cancel);
        });
    }
}
