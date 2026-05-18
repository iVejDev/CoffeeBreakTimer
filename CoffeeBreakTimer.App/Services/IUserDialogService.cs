namespace CoffeeBreakTimer.App.Services;

public interface IUserDialogService
{
    Task AlertAsync(
        string title,
        string message,
        string cancel,
        CancellationToken cancellationToken = default);

    Task<bool> ConfirmAsync(
        string title,
        string message,
        string accept,
        string cancel,
        CancellationToken cancellationToken = default);
}
