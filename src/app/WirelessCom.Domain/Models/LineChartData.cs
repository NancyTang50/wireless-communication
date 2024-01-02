using WirelessCom.Domain.Models.Entities;

namespace WirelessCom.Domain.Models;

public record LineChartData(
    Guid DeviceId,
    List<RoomClimateReading> Readings 
);