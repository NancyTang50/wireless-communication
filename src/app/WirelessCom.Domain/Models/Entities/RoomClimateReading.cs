namespace WirelessCom.Domain.Models.Entities;

public record RoomClimateReading(string RoomName, DateTime Timestamp, double Temperature, double Humidity) : BaseEntity;