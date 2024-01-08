using WirelessCom.Application.Services;

namespace WirelessCom.Infrastructure.Services;

public class BleRoomSensorNamingService : IBleRoomSensorNamingService
{
    public void SetName(Guid deviceId, string name)
    {
        Preferences.Set(GetDeviceNameKey(deviceId), name);
    }

    public string? GetName(Guid deviceId)
    {
        return Preferences.Get(GetDeviceNameKey(deviceId), null);
    }

    private static string GetDeviceNameKey(Guid deviceId)
    {
        return $"DeviceName_{deviceId}";
    }
}