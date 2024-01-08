namespace WirelessCom.Application.Services;

public interface IToastService
{
    /// <summary>
    ///     Show a toast message to the user.
    /// </summary>
    /// <param name="message">The message that will be shown in the toast.</param>
    /// <param name="fontSize">The font size of the toast message. Default 14.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous show operation.
    /// </returns>
    Task ShowToastAsync(string message, double fontSize = 14, CancellationToken cancellationToken = default);
}