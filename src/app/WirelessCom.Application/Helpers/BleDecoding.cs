namespace WirelessCom.Application.Helpers;

public static class BleDecoding
{
    public static double BleBytesToDouble(byte[] bytes)
    {
        if (bytes.Length != 2)
        {
            throw new ArgumentException("Byte array must be of length 2", nameof(bytes));
        }

        return BitConverter.ToInt16(bytes, 0) / 100.0;
    }

    public static DateTime BleBytesToDateTime(byte[] readingBytes)
    {
        if (readingBytes.Length != 10)
        {
            throw new ArgumentException("Byte array must be of length 10", nameof(readingBytes));
        }

        var year = BitConverter.ToUInt16(readingBytes, 0);
        var month = readingBytes[2];
        var day = readingBytes[3];
        var hour = readingBytes[4];
        var minute = readingBytes[5];
        var second = readingBytes[6];

        return new DateTime(
            year,
            month,
            day,
            hour,
            minute,
            second,
            DateTimeKind.Utc
        ).ToLocalTime();
    }
}