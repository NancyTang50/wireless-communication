using WirelessCom.Application.Database;
using WirelessCom.Application.Extensions;
using WirelessCom.Application.Helpers;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Entities;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.Services;

public class BleRoomSensorService : IBleRoomSensorService
{
    private static readonly SemaphoreSlim RegisterNotifySemaphore = new(1, 1);
    private readonly IBleService _bleService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly List<Guid> _roomSensorNotifying = new();
    private IReadOnlyList<BasicBleDevice> _roomSensors = new List<BasicBleDevice>();

    public BleRoomSensorService(IBleService bleService, IUnitOfWork unitOfWork)
    {
        _bleService = bleService;
        _unitOfWork = unitOfWork;
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

    public async Task<RoomClimateReading> ReadRoomClimate(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var temperature = await GetTemperature(deviceId, cancellationToken).ConfigureAwait(false);
        var humidity = await GetHumidity(deviceId, cancellationToken).ConfigureAwait(false);

        // TODO: Get timestamp from device
        var timestamp = DateTime.UtcNow;

        return new RoomClimateReading(deviceId, timestamp, Math.Round(temperature, 1), Math.Round(humidity, 1));
    }

    private async Task<double> GetTemperature(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var reading = await _bleService.ReadCharacteristicAsync(
                deviceId,
                BleServiceDefinitions.EnvironmentalService.ServiceGuid,
                BleServiceDefinitions.EnvironmentalService.TemperatureCharacteristicGuid,
                cancellationToken
            )
            .ConfigureAwait(false);

        return BleDecoding.BleBytesToDouble(reading.Bytes);
    }

    private async Task<double> GetHumidity(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var reading = await _bleService.ReadCharacteristicAsync(
                deviceId,
                BleServiceDefinitions.EnvironmentalService.ServiceGuid,
                BleServiceDefinitions.EnvironmentalService.HumidityCharacteristicGuid,
                cancellationToken
            )
            .ConfigureAwait(false);

        return BleDecoding.BleBytesToDouble(reading.Bytes);
    }

    private async Task BleServiceOnDevicesChangedEvent(object _, IReadOnlyList<BasicBleDevice> devices)
    {
        _roomSensors = devices.FilterByServiceId(
            BleServiceDefinitions.EnvironmentalService.ServiceIdPrefix,
            BleServiceDefinitions.TimeService.ServiceIdPrefix
        );

        await RegisterNotifyForRoomSensors().ConfigureAwait(false);
    }

    private async Task RegisterNotifyForRoomSensors()
    {
        await RegisterNotifySemaphore.WaitAsync();

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
                    SensorHumidityChanged
                );

                await _bleService.RegisterNotifyHandler(
                    roomSensor.Id,
                    BleServiceDefinitions.EnvironmentalService.ServiceGuid,
                    BleServiceDefinitions.EnvironmentalService.TemperatureCharacteristicGuid,
                    SensorTemperatureChanged
                );

                Console.WriteLine($"Registered notify for {roomSensor.Name}");
            }
        }
        finally
        {
            RegisterNotifySemaphore.Release();
        }
    }

    private async Task SensorHumidityChanged(BleCharacteristicReading reading)
    {
        await RoomClimateChanged(reading.DeviceId, null, BleDecoding.BleBytesToDouble(reading.Bytes)).ConfigureAwait(false);
    }

    private async Task SensorTemperatureChanged(BleCharacteristicReading reading)
    {
        await RoomClimateChanged(reading.DeviceId, BleDecoding.BleBytesToDouble(reading.Bytes), null).ConfigureAwait(false);
    }

    private async Task RoomClimateChanged(Guid deviceId, double? temperature, double? humidity)
    {
        temperature ??= await GetTemperature(deviceId).ConfigureAwait(false);
        humidity ??= await GetHumidity(deviceId).ConfigureAwait(false);

        // TODO: Get timestamp from device
        var timestamp = DateTime.UtcNow;

        var roomClimateReading = new RoomClimateReading(deviceId, timestamp, Math.Round(temperature.Value, 1), Math.Round(humidity.Value, 1));
        await _unitOfWork.RoomClimateReading.AddAsync(roomClimateReading);
        await _unitOfWork.SaveChangesAsync();
    }
}