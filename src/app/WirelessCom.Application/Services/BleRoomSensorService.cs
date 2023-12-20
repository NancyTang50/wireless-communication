using WirelessCom.Application.Caching;
using WirelessCom.Application.Extensions;
using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;
using WirelessCom.Domain.Services;

namespace WirelessCom.Application.Services;

public class BleRoomSensorService : IBleRoomSensorService
{
    private readonly IBleService _bleService;
    private IReadOnlyList<BasicBleDevice> _bleDevices = new List<BasicBleDevice>();

    public BleRoomSensorService(IBleService bleService)
    {
        _bleService = bleService;
        _bleService.OnDevicesChangedEvent += BleServiceOnOnDevicesChangedEvent;
    }

    public async Task ScanForDevices(CancellationToken cancellationToken = default)
    {
        await _bleService.ScanForDevices(
                new[] { BleServiceDefinitions.EnvironmentalService.ServiceGuid, BleServiceDefinitions.TimeService.ServiceGuid },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private Task BleServiceOnOnDevicesChangedEvent(object _, IReadOnlyList<BasicBleDevice> devices)
    {
        _bleDevices = devices.FilterByServiceId(BleServiceDefinitions.EnvironmentalService.ServiceIdPrefix, BleServiceDefinitions.TimeService.ServiceIdPrefix);
        return Task.CompletedTask;
    }
}