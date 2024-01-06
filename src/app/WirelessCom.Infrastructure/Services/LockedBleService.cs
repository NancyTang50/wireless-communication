using Plugin.BLE.Abstractions.Contracts;
using WirelessCom.Application.Caching;
using WirelessCom.Application.Services;
using WirelessCom.Domain.Models;
using BluetoothState = WirelessCom.Domain.Models.Enums.BluetoothState;

namespace WirelessCom.Infrastructure.Services;

// Yes this incredibly weird to do.
// However, the BLE plugin is not thread safe and will throw randomly and will get stuck if you try to do multiple things at once.
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

    public Task<IEnumerable<BasicBleService>> GetServicesAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
        ExecuteWithDeviceLock(() => _bleService.GetServicesAsync(deviceId, cancellationToken), deviceId, 10);

    public Task ConnectDeviceByIdAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
        ExecuteWithDeviceLock(() => _bleService.ConnectDeviceByIdAsync(deviceId, cancellationToken), deviceId);

    public Task DisconnectDeviceByIdAsync(Guid deviceId) => ExecuteWithDeviceLock(() => _bleService.DisconnectDeviceByIdAsync(deviceId), deviceId, 5);

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

    public Task<int> WriteCharacteristicAsync(
        Guid deviceId,
        Guid serviceId,
        Guid characteristicId,
        byte[] data,
        CancellationToken cancellationToken = default
    ) =>
        ExecuteWithDeviceLock(() => _bleService.WriteCharacteristicAsync(deviceId, serviceId, characteristicId, data, cancellationToken), deviceId);

    private async Task<T> ExecuteWithDeviceLock<T>(Func<Task<T>> task, Guid deviceId, int timeout = 20)
    {
        var semaphore = GetDeviceSemaphores(deviceId);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));

        try
        {
            var resultTask = Task.Run(
                async () =>
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);

                    try
                    {
                        return await task().ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                },
                cts.Token
            );

            // Wait for either the task to complete or the timeout
            await resultTask.ConfigureAwait(false);
            return resultTask.Result;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"Task cancelled after {timeout} seconds: {ex.Message}");
            throw; // Rethrow the exception or handle as needed
        }
    }

    private async Task ExecuteWithDeviceLock(Func<Task> task, Guid deviceId, int timeout = 20)
    {
        var semaphore = GetDeviceSemaphores(deviceId);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));

        try
        {
            var resultTask = Task.Run(
                async () =>
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);

                    try
                    {
                        await task().ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                },
                cts.Token
            );

            // Wait for either the task to complete or the timeout
            await resultTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"Task cancelled after {timeout} seconds: {ex.Message}");
            throw; // Rethrow the exception or handle as needed
        }
    }

    private SemaphoreSlim GetDeviceSemaphores(Guid deviceId)
    {
        var semaphore = _deviceSemaphores.GetAll().FirstOrDefault().Value;
        if (semaphore is not null)
        {
            return semaphore;
        }

        semaphore = new SemaphoreSlim(1, 1);
        _deviceSemaphores.AddOrUpdate(deviceId, semaphore);

        return semaphore;
    }
}