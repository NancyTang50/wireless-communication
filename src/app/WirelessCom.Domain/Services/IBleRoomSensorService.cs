using WirelessCom.Domain.Models.Entities;

namespace WirelessCom.Domain.Services;

public interface IBleRoomSensorService
{
    Task ScanForRoomSensors(CancellationToken cancellationToken = default);
    Task<RoomClimateReading> ReadRoomClimate(Guid deviceId, CancellationToken cancellationToken = default);
}