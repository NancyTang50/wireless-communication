using WirelessCom.Application.Extensions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.Services;

public class BleRoomSensorService : IBleRoomSensorService
{
    private static readonly SemaphoreSlim RegisterNotifySemaphore = new(1, 1);
    private readonly IBleService _bleService;
    private readonly List<Guid> _roomSensorNotifying = new();
    private IReadOnlyList<BasicBleDevice> _roomSensors = new List<BasicBleDevice>();

    public BleRoomSensorService(IBleService bleService)
    {
        _bleService = bleService;
        _bleService.OnDevicesChangedEvent += BleServiceOnDevicesChangedEvent;
    }

    public async Task ScanForRoomSensors(CancellationToken cancellationToken = default)
    {
        await _bleService.ScanForDevices(
                new[] { BleServiceDefinitions.EnvironmentalService.ServiceGuid, BleServiceDefinitions.TimeService.ServiceGuid },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private async Task BleServiceOnDevicesChangedEvent(object _, IReadOnlyList<BasicBleDevice> devices)
    {
        _roomSensors = devices.FilterByServiceId(
            BleServiceDefinitions.EnvironmentalService.ServiceIdPrefix,
            BleServiceDefinitions.TimeService.ServiceIdPrefix
        );

        await RegisterNotifyForRoomSensors().ConfigureAwait(false);
    }

    private async Task RegisterNotifyForRoomSensors(CancellationToken cancellationToken = default)
    {
        await RegisterNotifySemaphore.WaitAsync(cancellationToken);

        try
        {
            foreach (var roomSensor in _roomSensors.Where(x => x.IsConnected && x.IsRoomSensor()))
            {
                if (_roomSensorNotifying.Contains(roomSensor.Id))
                {
                    // Already notifying
                    continue;
                }

                _roomSensorNotifying.Add(roomSensor.Id);

                await _bleService.RegisterNotifyHandler(
                    roomSensor.Id,
                    BleServiceDefinitions.EnvironmentalService.ServiceGuid,
                    BleServiceDefinitions.EnvironmentalService.HumidityCharacteristicGuid,
                    SensorHumidityChanged,
                    cancellationToken
                );

                await _bleService.RegisterNotifyHandler(
                    roomSensor.Id,
                    BleServiceDefinitions.EnvironmentalService.ServiceGuid,
                    BleServiceDefinitions.EnvironmentalService.TemperatureCharacteristicGuid,
                    SensorTemperatureChanged,
                    cancellationToken
                );

                Console.WriteLine($"Registered notify for {roomSensor.Name}");
            }
        }
        finally
        {
            RegisterNotifySemaphore.Release();
        }
    }

    private Task SensorHumidityChanged(BleCharacteristicReading reading)
    {
        Console.WriteLine($"Humidity: {reading.Bytes}");
        return Task.CompletedTask;
    }

    private Task SensorTemperatureChanged(BleCharacteristicReading reading)
    {
        Console.WriteLine($"Temperature: {reading.Bytes}");
        return Task.CompletedTask;
    }
}