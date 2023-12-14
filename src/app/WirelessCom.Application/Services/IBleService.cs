using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.Services;

public interface IBleService
{
    /// <summary>
    ///     Delegate for the <see cref="OnBleStateChangedEvent" /> event.
    /// </summary>
    public delegate void OnBleStateChanged(object source, BluetoothState bluetoothState);

    /// <summary>
    ///     Delegate for the <see cref="OnDevicesChangedEvent" /> event.
    /// </summary>
    public delegate Task? OnDevicesChanged(object source, IReadOnlyList<BasicBleDevice> bleDevices);

    /// <summary>
    ///     Event that is triggered when the Bluetooth state changes.
    /// </summary>
    public event OnBleStateChanged OnBleStateChangedEvent;

    /// <summary>
    ///     Event that is triggered when the Bluetooth state changes.
    /// </summary>
    public event OnDevicesChanged OnDevicesChangedEvent;

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
    ///     The current state of the Bluetooth adapter.
    /// </returns>
    BluetoothState GetBluetoothState();

    /// <summary>
    ///     Starts scanning for Bluetooth devices with the given <paramref name="guids" />.
    /// </summary>
    /// <param name="guids">Optional: The service UUIDs to scan for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous scanning operation.
    ///     Returns when the scanning is done.
    /// </returns>
    Task ScanForDevices(Guid[]? guids = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a list of all services of the device with the given <paramref name="deviceId" />.
    /// </summary>
    /// <param name="deviceId">The id of the device.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation.
    ///     The Result will contain a list of all available <see cref="BasicBleService" />s.
    /// </returns>
    Task<IReadOnlyList<BasicBleService>> GetServicesAsync(Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a list of all <see cref="BareBleAdvertisement" />s of the device with the given <paramref name="deviceId" />.
    /// </summary>
    /// <param name="deviceId">The id of the device.</param>
    /// <returns>
    ///     A list of all <see cref="BareBleAdvertisement" />s of the device with the given <paramref name="deviceId" />.
    /// </returns>
    IReadOnlyList<BareBleAdvertisement> GetBareBleAdvertisements(Guid deviceId);

    /// <summary>
    ///     Connects to the device with the given <paramref name="deviceId" />.
    /// </summary>
    /// <param name="deviceId">The id of the device.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous connect operation.
    /// </returns>
    Task ConnectDeviceByIdAsync(Guid deviceId, CancellationToken cancellationToken = default);
}