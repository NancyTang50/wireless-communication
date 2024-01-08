using WirelessCom.Domain.Models;

namespace WirelessCom.Application.Extensions;

public static class BasicBleDeviceExtensions
{
    public static bool IsRoomSensor(this BasicBleDevice device)
    {
        return !string.IsNullOrWhiteSpace(device.Name) && device.Name.Contains("RoomSensor", StringComparison.InvariantCultureIgnoreCase);
    }
}