using WirelessCom.Application.Caching;
using WirelessCom.Application.Database;
using WirelessCom.Application.Extensions;
using WirelessCom.Application.Helpers;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Entities;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.Services;

public class BleRoomSensorService : IBleRoomSensorService
{
    private static readonly SemaphoreSlim UpdateNotifySemaphore = new(1, 1), UpdateTimeSemaphore = new(1, 1);
    private readonly IBleService _bleService;
    private readonly GenericConcurrentDictionary<Guid, RoomClimateReading> _previousRoomClimateReadings = new();
    private readonly List<Guid> _roomSensorNotifying = new();
    private readonly List<Guid> _roomSensorTimeUpdated = new();
    private readonly IUnitOfWork _unitOfWork;
    private IReadOnlyList<BasicBleDevice> _roomSensors = new List<BasicBleDevice>();

    public BleRoomSensorService(IBleService bleService, IUnitOfWork unitOfWork)
    {
        _bleService = bleService;
        _unitOfWork = unitOfWork;
        _bleService.OnDevicesChangedEvent += BleServiceOnDevicesChangedEvent;
    }

    /// <inheritdoc />
    public event IBleRoomSensorService.OnNewReadingReceived? OnNewReadingReceivedEvent;

    /// <inheritdoc />
    public async Task ScanForRoomSensors(CancellationToken cancellationToken = default)
    {
        await _bleService.ScanForDevices(
                [BleServiceDefinitions.EnvironmentalService.ServiceGuid, BleServiceDefinitions.TimeService.ServiceGuid],
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<RoomClimateReading> ReadRoomClimate(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var temperature = await GetTemperature(deviceId, cancellationToken).ConfigureAwait(false);
        var humidity = await GetHumidity(deviceId, cancellationToken).ConfigureAwait(false);
        var timestamp = await ReadSensorDateTime(deviceId, cancellationToken).ConfigureAwait(false);

        var reading = new RoomClimateReading(deviceId, timestamp, Math.Round(temperature, 1), Math.Round(humidity, 1));
        await _unitOfWork.RoomClimateReading.AddAsync(reading);
        await _unitOfWork.SaveChangesAsync();

        return reading;
    }

    /// <inheritdoc />
    public async Task<DateTime> ReadSensorDateTime(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var reading = await _bleService.ReadCharacteristicAsync(
                deviceId,
                BleServiceDefinitions.TimeService.ServiceGuid,
                BleServiceDefinitions.TimeService.CurrentTimeCharacteristicGuid,
                cancellationToken
            )
            .ConfigureAwait(false);

        return BleDecoding.BleBytesToDateTime(reading.Bytes);
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

    private async Task BleServiceOnDevicesChangedEvent(object _, IEnumerable<BasicBleDevice> devices)
    {
        var roomSensors = devices.Where(x => x.IsRoomSensor()).ToList();
        if (roomSensors.SequenceEqual(_roomSensors))
        {
            return;
        }

        _roomSensors = roomSensors;

        await UpdateNotifyForRoomSensors().ConfigureAwait(false);
        await UpdateLocalTime().ConfigureAwait(false);
    }

    private async Task UpdateLocalTime()
    {
        await UpdateTimeSemaphore.WaitAsync();

        try
        {
            foreach (var roomSensor in _roomSensors.Where(x => x.IsConnected && x.IsRoomSensor()))
            {
                if (_roomSensorTimeUpdated.Contains(roomSensor.Id))
                {
                    // Already notifying
                    continue;
                }

                _roomSensorTimeUpdated.Add(roomSensor.Id);

                await _bleService.WriteCharacteristicAsync(
                    roomSensor.Id,
                    BleServiceDefinitions.TimeService.ServiceGuid,
                    BleServiceDefinitions.TimeService.CurrentTimeCharacteristicGuid,
                    BleEncoding.GetCurrentDateTimeBleBytes()
                );
            }
        }
        finally
        {
            UpdateTimeSemaphore.Release();
        }
    }

    private async Task UpdateNotifyForRoomSensors()
    {
        await UpdateNotifySemaphore.WaitAsync();

        try
        {
            foreach (var roomSensor in _roomSensors.Where(x => _roomSensorNotifying.Contains(x.Id) && !x.IsConnected))
            {
                // Remove notify handlers for disconnected devices
                _roomSensorNotifying.Remove(roomSensor.Id);
            }

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
            }
        }
        finally
        {
            UpdateNotifySemaphore.Release();
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
        // Only get temperature and humidity if no previous reading is available.
        // This is to avoid reading the same value twice. New changes will be captured by the notify events.
        var previousReading = _previousRoomClimateReadings.Get(deviceId);
        temperature ??= previousReading?.Temperature ?? await GetTemperature(deviceId).ConfigureAwait(false);
        humidity ??= previousReading?.Humidity ?? await GetHumidity(deviceId).ConfigureAwait(false);
        var timestamp = await ReadSensorDateTime(deviceId).ConfigureAwait(false);

        var reading = new RoomClimateReading(deviceId, timestamp, Math.Round(temperature.Value, 1), Math.Round(humidity.Value, 1));
        await _unitOfWork.RoomClimateReading.AddAsync(reading);
        await _unitOfWork.SaveChangesAsync();

        _previousRoomClimateReadings.AddOrUpdate(deviceId, reading);
        OnNewReadingReceivedEvent?.Invoke(this, reading);
    }
}