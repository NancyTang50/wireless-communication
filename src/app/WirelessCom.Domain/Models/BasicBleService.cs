namespace WirelessCom.Domain.Models;

public record BasicBleService(Guid Id, Guid DeviceId, string Name, bool IsPrimary);