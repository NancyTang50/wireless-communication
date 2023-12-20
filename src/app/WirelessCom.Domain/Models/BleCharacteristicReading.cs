namespace WirelessCom.Domain.Models;

/// <summary>
///     Represents a BLE characteristic reading.
/// </summary>
/// <param name="Bytes">The raw bytes of the reading.</param>
public record BleCharacteristicReading(IReadOnlyList<byte> Bytes);