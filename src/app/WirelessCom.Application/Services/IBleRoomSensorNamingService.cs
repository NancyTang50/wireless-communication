namespace WirelessCom.Application.Services;

public interface IBleRoomSensorNamingService
{
    void SetName(Guid deviceId, string name);
    string? GetName(Guid deviceId);
}