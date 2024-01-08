using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.Services;

/// <summary>
///     The Bluetooth Low Energy service.
///     Contains all the methods to interact with Bluetooth Low Energy devices.
/// </summary>
public interface IBleService
{
    /// <summary>
    ///     Delegate for when a notify for a characteristic is received.
    /// </summary>
    public delegate Task NotifyCharacteristicUpdated(BleCharacteristicReading reading);

    /// <summary>
    ///     Delegate for the <see cref="OnBleStateChangedEvent" /> event.
    /// </summary>
    public delegate void OnBleStateChanged(object source, BluetoothState bluetoothState);

    /// <summary>
    ///     Delegate for the <see cref="OnDevicesChangedEvent" /> event.
    /// </summary>
    public delegate Task? OnDevicesChanged(object source, IEnumerable<BasicBleDevice> bleDevices);

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
    /// <remarks>
    ///     The device must be connected before calling this method.
    /// </remarks>
    /// <param name="deviceId">The id of the device.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation.
    ///     The Result will contain a list of all available <see cref="BasicBleService" />s.
    /// </returns>
    Task<IEnumerable<BasicBleService>> GetServicesAsync(Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Connects to the device with the given <paramref name="deviceId" />.
    /// </summary>
    /// <param name="deviceId">The id of the device.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous connect operation.
    ///     Returns when the device is connected.
    /// </returns>
    Task ConnectDeviceByIdAsync(Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Disconnects from the device with the given <paramref name="deviceId" />.
    /// </summary>
    /// <param name="deviceId">The id of the device.</param>
    /// <returns>
    ///     A task that represents the asynchronous disconnect operation.
    ///     Returns when the device is disconnected.
    /// </returns>
    Task DisconnectDeviceByIdAsync(Guid deviceId);

    /// <summary>
    ///     Returns a list of all <see cref="BasicBleDevice" />s.
    /// </summary>
    List<BasicBleDevice> GetAllBasicBleDevices();

    /// <summary>
    ///     Read a characteristic from a device.
    /// </summary>
    /// <remarks>
    ///     The device must be connected before calling this method.
    /// </remarks>
    /// <param name="deviceId">The id of the device.</param>
    /// <param name="serviceId">The id of the service where the characteristic is located.</param>
    /// <param name="characteristicId">The id of the characteristic to read.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation.
    ///     The Result will contain the <see cref="BleCharacteristicReading" />.
    /// </returns>
    Task<BleCharacteristicReading> ReadCharacteristicAsync(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Registers a handler for notify events of a characteristic.
    /// </summary>
    /// <remarks>
    ///     The device must be connected before calling this method.
    /// </remarks>
    /// <param name="deviceId">The id of the device.</param>
    /// <param name="serviceId">The id of the service where the characteristic is located.</param>
    /// <param name="characteristicId">The id of the characteristic to handle the notify events for.</param>
    /// <param name="handler">The handler that will be called when a notify event is received.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous register operation.
    /// </returns>
    Task RegisterNotifyHandler(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        NotifyCharacteristicUpdated handler,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Writes data to a characteristic.
    /// </summary>
    /// <param name="deviceId">The id of the device where to data should be written to.</param>
    /// <param name="serviceId">The id of the service where the characteristic is located.</param>
    /// <param name="characteristicId">The id of the characteristic to write to.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous write operation.
    ///     The result will contain the status code of the write operation. (0 = successful)
    /// </returns>
    Task<int> WriteCharacteristicAsync(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        byte[] data,
        CancellationToken cancellationToken = default
    );
}