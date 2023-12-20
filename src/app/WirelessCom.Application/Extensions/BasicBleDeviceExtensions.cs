using WirelessCom.Domain.Models;
using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Application.Extensions;

public static class BasicBleDeviceExtensions
{
    // Todo: This is pure garbage, but it works for now. We need to have a better way of filtering. For example a better device name "RoomSensor-<4 random chars>"
    public static IReadOnlyList<BasicBleDevice> FilterByServiceId(this IEnumerable<BasicBleDevice> devices, params int[] serviceIdPrefixes)
    {
        return serviceIdPrefixes.Aggregate(
            devices.ToList(),
            (current, serviceIdPrefix) => current.Where(
                    x => x.Advertisements.Any(
                        z => z.Type == BleAdvertisementType.UuidsComplete16Bit &&
                            ConvertBytesToIntegers(z.Data).Any(y => y == serviceIdPrefix)
                    )
                )
                .ToList()
        );
    }

    public static bool IsRoomSensor(this BasicBleDevice device)
    {
        var roomSensorServiceIdPrefix = new[]
        {
            BleServiceDefinitions.EnvironmentalService.ServiceIdPrefix,
            BleServiceDefinitions.TimeService.ServiceIdPrefix
        };

        return device.Advertisements.Any(
            z => z.Type == BleAdvertisementType.UuidsComplete16Bit &&
                ConvertBytesToIntegers(z.Data).Any(roomSensorServiceIdPrefix.Contains)
        );
    }

    private static IEnumerable<int> ConvertBytesToIntegers(IReadOnlyList<byte> bytes)
    {
        for (var i = 0; i < bytes.Count; i += 2)
        {
            yield return (bytes[i] << 8) | (i + 1 < bytes.Count ? bytes[i + 1] : 0);
        }
    }
}