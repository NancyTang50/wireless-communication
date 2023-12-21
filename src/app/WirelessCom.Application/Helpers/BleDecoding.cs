namespace WirelessCom.Application.Helpers;

public static class BleDecoding
{
    public static decimal BleBytesToInt(byte[] bytes)
    {
        if (bytes.Length != 2)
        {
            throw new ArgumentException("Byte array must be of length 2", nameof(bytes));
        }

        return BitConverter.ToInt16(bytes, 0) / 100.0m;
    }
}