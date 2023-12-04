using WirelessCom.Domain.Enums;

namespace WirelessCom.Application.Services;

public interface IBleService
{
    /// <summary>
    ///     Delegate for the <see cref="OnBleStateChangedEvent"/> event.
    /// </summary>
    public delegate void OnBleStateChanged(object source, BluetoothState bluetoothState);
    
    /// <summary>
    ///     Event that is triggered when the Bluetooth state changes.
    /// </summary>
    public event OnBleStateChanged OnBleStateChangedEvent;

    /// <summary>
    ///     Checks if the app has the correct permissions to use Bluetooth.
    ///     If not, it will ask the user for permission.
    /// </summary>
    /// <returns>
    ///     True if the app has the correct permissions, false otherwise.
    /// </returns>
    Task<bool> HasCorrectPermissions();
    
    /// <summary>
    ///     Returns the current state of the Bluetooth adapter.
    /// </summary>
    /// <returns>
    ///    The current state of the Bluetooth adapter.
    /// </returns>
    BluetoothState GetBluetoothState();
}