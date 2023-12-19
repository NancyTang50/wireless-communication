namespace WirelessCom.Domain.Models;

public record BleCharacteristicReading(IReadOnlyList<byte> Bytes, int ResultCode);