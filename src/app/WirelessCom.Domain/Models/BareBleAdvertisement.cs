using WirelessCom.Domain.Models.Enums;

namespace WirelessCom.Domain.Models;

public record BareBleAdvertisement(BleAdvertisementType Type, byte[] Data)
{
    public override string ToString()
    {
        return $"Type: {Type}, Data: {BitConverter.ToString(Data)}";
    }
}