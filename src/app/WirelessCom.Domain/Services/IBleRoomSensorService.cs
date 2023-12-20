namespace WirelessCom.Domain.Services;

public interface IBleRoomSensorService
{
    Task ScanForRoomSensors(CancellationToken cancellationToken = default);
}