namespace CoffeeBreakTimer.App.Services;

public interface IUserDialogService
{
    Task<bool> ConfirmAsync(
        string title,
        string message,
        string accept,
        string cancel,
        CancellationToken cancellationToken = default);
}
