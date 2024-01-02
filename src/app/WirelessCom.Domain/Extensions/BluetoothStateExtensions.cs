using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Domain.Extensions;

public static class BluetoothStateExtensions
{
    public static string ToReadableString(this BluetoothState bluetoothState)
    {
        return bluetoothState switch
        {
            BluetoothState.Unknown => "Unknown",
            BluetoothState.Unavailable => "Unavailable",
            BluetoothState.Unauthorized => "Unauthorized",
            BluetoothState.TurningOn => "Turning on",
            BluetoothState.On => "On",
            BluetoothState.TurningOff => "Turning off",
            BluetoothState.Off => "Off",
            _ => throw new ArgumentOutOfRangeException(nameof(bluetoothState), bluetoothState, null)
        };
    }
}