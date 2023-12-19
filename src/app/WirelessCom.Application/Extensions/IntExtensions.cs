namespace WirelessCom.Application.Extensions;

public static class IntExtensions
{
    public static Guid ToBleGuid(this int value)
    {
        return new Guid(value, 0, 0x1000, new byte[] { 0x80, 0, 0, 0x80, 0x5f, 0x9b, 0x34, 0xfb });
    }
}