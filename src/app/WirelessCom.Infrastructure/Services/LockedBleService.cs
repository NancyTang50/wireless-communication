using Plugin.BLE.Abstractions.Contracts;
using WirelessCom.Application.Caching;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Models;
using BluetoothState = WirelessCom.Domain.Models.Enums.BluetoothState;

namespace WirelessCom.Infrastructure.Services;

// Yes this incredibly weird to do. However, the BLE plugin is not thread safe and will throw randomly get stuck if you try to do multiple things at once.
public class LockedBleService : IBleService
{
    private readonly BleService _bleService;
    private readonly GenericConcurrentDictionary<Guid, SemaphoreSlim> _deviceSemaphores = new();

    public LockedBleService(IBluetoothLE bluetoothLe, IAdapter adapter)
    {
        _bleService = new BleService(bluetoothLe, adapter);
    }

    public event IBleService.OnBleStateChanged? OnBleStateChangedEvent
    {
        add => _bleService.OnBleStateChangedEvent += value;
        remove => _bleService.OnBleStateChangedEvent -= value;
    }

    public event IBleService.OnDevicesChanged? OnDevicesChangedEvent
    {
        add => _bleService.OnDevicesChangedEvent += value;
        remove => _bleService.OnDevicesChangedEvent -= value;
    }

    public Task<bool> HasCorrectPermissions() => _bleService.HasCorrectPermissions();

    public BluetoothState GetBluetoothState() => _bleService.GetBluetoothState();

    public Task ScanForDevices(Guid[]? guids = null, CancellationToken cancellationToken = default) => _bleService.ScanForDevices(guids, cancellationToken);

    public List<BasicBleDevice> GetAllBasicBleDevices() => _bleService.GetAllBasicBleDevices();

    public Task<IReadOnlyList<BasicBleService>> GetServicesAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
        ExecuteWithDeviceLock(() => _bleService.GetServicesAsync(deviceId, cancellationToken), deviceId);

    public Task ConnectDeviceByIdAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
        ExecuteWithDeviceLock(() => _bleService.ConnectDeviceByIdAsync(deviceId, cancellationToken), deviceId);

    public Task<BleCharacteristicReading> ReadCharacteristicAsync(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        CancellationToken cancellationToken = default
    ) =>
        ExecuteWithDeviceLock(() => _bleService.ReadCharacteristicAsync(deviceId, serviceId, characteristicId, cancellationToken), deviceId);

    public Task RegisterNotifyHandler(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        IBleService.NotifyCharacteristicUpdated handler,
        CancellationToken cancellationToken = default
    ) =>
        ExecuteWithDeviceLock(() => _bleService.RegisterNotifyHandler(deviceId, serviceId, characteristicId, handler, cancellationToken), deviceId);

    private async Task<T> ExecuteWithDeviceLock<T>(Func<Task<T>> task, Guid deviceId)
    {
        var semaphore = GetDeviceSemaphores(deviceId);
        await semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            var result = await task().ConfigureAwait(false);

            return result;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ExecuteWithDeviceLock(Func<Task> task, Guid deviceId)
    {
        var semaphore = GetDeviceSemaphores(deviceId);
        await semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            await task().ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private SemaphoreSlim GetDeviceSemaphores(Guid deviceId)
    {
        var semaphore = _deviceSemaphores.Get(deviceId);
        if (semaphore is not null)
        {
            return semaphore;
        }

        semaphore = new SemaphoreSlim(1, 1);
        _deviceSemaphores.AddOrUpdate(deviceId, semaphore);

        return semaphore;
    }
}