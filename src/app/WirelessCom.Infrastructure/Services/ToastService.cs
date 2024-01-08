using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using WirelessCom.Application.Services;

namespace WirelessCom.Infrastructure.Services;

public class ToastService : IToastService
{
    /// <inheritdoc />
    public async Task ShowToastAsync(string message, double fontSize = 14, CancellationToken cancellationToken = default)
    {
        var toast = Toast.Make(message, ToastDuration.Short, fontSize);
        var dispatcher = Dispatcher.GetForCurrentThread();
        if (dispatcher is null)
        {
            // This is the un-preferred way to do this.
            await MainThread.InvokeOnMainThreadAsync(LocalShowToastAsync).ConfigureAwait(false);
            return;
        }
        
        await dispatcher.DispatchAsync(LocalShowToastAsync).ConfigureAwait(false);
        return;

        Task LocalShowToastAsync()
        {
            return toast.Show(cancellationToken);
        }
    }
}