using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using WirelessCom.Application.Caching;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Exceptions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;
using BluetoothState = WirelessCom.Domain.Models.Enums.BluetoothState;

namespace WirelessCom.Infrastructure.Services;

public class BleService : IBleService
{
    private readonly IAdapter _adapter;
    private readonly IBluetoothLE _bluetoothLe;
    private readonly GenericConcurrentDictionary<Guid, IDevice> _devices = new();

    public BleService(IBluetoothLE bluetoothLe, IAdapter adapter)
    {
        _bluetoothLe = bluetoothLe;
        _adapter = adapter;

        InitOnBleStateChanged();
        InitOnDevicesChangedEvent();
        InitOnBleDeviceConnectionChangedEvents();
    }

    /// <inheritdoc />
    public event IBleService.OnBleStateChanged? OnBleStateChangedEvent;

    /// <inheritdoc />
    public event IBleService.OnDevicesChanged? OnDevicesChangedEvent;

    /// <inheritdoc />
    public async Task<bool> HasCorrectPermissions()
    {
        if (await Permissions.RequestAsync<Permissions.Bluetooth>().ConfigureAwait(false) == PermissionStatus.Granted)
        {
            return true;
        }

        AppInfo.ShowSettingsUI();
        return false;
    }

    /// <inheritdoc />
    public BluetoothState GetBluetoothState()
    {
        return (BluetoothState)_bluetoothLe.State;
    }

    /// <inheritdoc />
    public async Task ScanForDevices(Guid[]? guids, CancellationToken cancellationToken = default)
    {
        var scanFilterOptions = new ScanFilterOptions();
        if (guids != null)
        {
            scanFilterOptions.ServiceUuids = guids;
        }

        _adapter.ScanMode = ScanMode.LowLatency;
        await _adapter.StartScanningForDevicesAsync(scanFilterOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BasicBleService>> GetServicesAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = _devices.Get(deviceId) ?? throw new BleDeviceNotFoundException(deviceId);
        ValidateBleConnected(deviceId, device);

        var services = await device.GetServicesAsync(cancellationToken).ConfigureAwait(false);
        return services.Select(service => new BasicBleService(service.Id, service.Device.Id, service.Name, service.IsPrimary)).ToList();
    }

    /// <inheritdoc />
    public async Task ConnectDeviceByIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var parameters = new ConnectParameters(forceBleTransport: true);
        var device = _devices.Get(deviceId) ?? throw new BleDeviceNotFoundException(deviceId);
        await _adapter.ConnectToDeviceAsync(device, parameters, cancellationToken).ConfigureAwait(false);
    }
    
    /// <inheritdoc />
    public async Task DisconnectDeviceByIdAsync(Guid deviceId)
    {
        var device = _devices.Get(deviceId) ?? throw new BleDeviceNotFoundException(deviceId);
        await _adapter.DisconnectDeviceAsync(device).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public List<BasicBleDevice> GetAllBasicBleDevices()
    {
        return _devices.GetAll()
            .Select(
                device => new BasicBleDevice(
                    device.Key,
                    device.Value.Name,
                    _adapter.ConnectedDevices.Any(x => x.Id == device.Key),
                    device.Value.Rssi,
                    device.Value.AdvertisementRecords.Select(record => new BareBleAdvertisement((BleAdvertisementType)record.Type, record.Data)).ToList(),
                    null
                )
            )
            .ToList();
    }

    /// <inheritdoc />
    public async Task<BleCharacteristicReading> ReadCharacteristicAsync(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        CancellationToken cancellationToken = default
    )
    {
        var device = _devices.Get(deviceId) ?? throw new BleDeviceNotFoundException(deviceId);
        ValidateBleConnected(deviceId, device);

        var service = await device.GetServiceAsync(serviceId, cancellationToken).ConfigureAwait(false);
        var characteristic = await service.GetCharacteristicAsync(characteristicId).ConfigureAwait(false);
        var result = await characteristic.ReadAsync(cancellationToken).ConfigureAwait(false);

        return new BleCharacteristicReading(deviceId, result.data);
    }

    /// <inheritdoc />
    public async Task RegisterNotifyHandler(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        IBleService.NotifyCharacteristicUpdated handler,
        CancellationToken cancellationToken = default
    )
    {
        var device = _devices.Get(deviceId) ?? throw new BleDeviceNotFoundException(deviceId);
        ValidateBleConnected(deviceId, device);

        var service = await device.GetServiceAsync(serviceId, cancellationToken).ConfigureAwait(false);
        var characteristic = await service.GetCharacteristicAsync(characteristicId).ConfigureAwait(false);

        if (!characteristic.CanUpdate)
        {
            throw new BleCharacteristicNotifiableException(characteristicId);
        }

        await characteristic.StartUpdatesAsync(cancellationToken).ConfigureAwait(false);
        characteristic.ValueUpdated += (_, args) => handler(new BleCharacteristicReading(deviceId, args.Characteristic.Value));
    }

    private void ValidateBleConnected(Guid deviceId, IDevice device)
    {
        if (!_bluetoothLe.Adapter.ConnectedDevices.Contains(device))
        {
            throw new RequireBleConnectionException(deviceId);
        }
    }

    private void InitOnBleStateChanged()
    {
        _bluetoothLe.StateChanged += (_, args) => OnBleStateChangedEvent?.Invoke(this, (BluetoothState)args.NewState);
    }

    private void InitOnDevicesChangedEvent()
    {
        _devices.ItemAdded += (_, _) => OnDevicesChangedEvent?.Invoke(this, GetAllBasicBleDevices());
        _devices.ItemRemoved += (_, _) => OnDevicesChangedEvent?.Invoke(this, GetAllBasicBleDevices());
    }

    private void InitOnBleDeviceConnectionChangedEvents()
    {
        _adapter.DeviceDiscovered += (_, a) => _devices.AddOrUpdate(a.Device.Id, a.Device);
        _adapter.DeviceConnected += (_, a) => _devices.AddOrUpdate(a.Device.Id, a.Device);
        _adapter.DeviceDisconnected += (_, a) => _devices.AddOrUpdate(a.Device.Id, a.Device);
        _adapter.DeviceConnectionLost += (_, a) => _devices.AddOrUpdate(a.Device.Id, a.Device);
        _adapter.DeviceConnectionError += (_, a) => _devices.AddOrUpdate(a.Device.Id, a.Device);
    }
}