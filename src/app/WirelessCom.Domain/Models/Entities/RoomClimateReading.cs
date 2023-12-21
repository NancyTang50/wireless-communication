namespace WirelessCom.Domain.Models.Entities;

public record RoomClimateReading(Guid DeviceId, DateTime Timestamp, double Temperature, double Humidity) : BaseEntity;