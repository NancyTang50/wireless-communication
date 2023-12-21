namespace WirelessCom.Domain.Models;

/// <summary>
///     Represents a BLE characteristic reading.
/// </summary>
/// <param name="DeviceId">The ID of the device that sent the reading.</param>
/// <param name="Bytes">The raw bytes of the reading.</param>
public record BleCharacteristicReading(Guid DeviceId, byte[] Bytes);