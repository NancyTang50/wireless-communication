namespace WirelessCom.Application.Extensions;

public static class IntExtensions
{
    public static Guid ToGuid(this int value)
    {
        return new Guid(BitConverter.GetBytes(value));
    }
}