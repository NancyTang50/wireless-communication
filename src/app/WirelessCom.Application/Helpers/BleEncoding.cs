namespace WirelessCom.Application.Helpers;

public class BleEncoding
{
    public static byte[] GetCurrentDateTimeBleBytes()
    {
        return GetDateTimeBleBytes(DateTime.Now);
    }

    public static byte[] GetDateTimeBleBytes(DateTime dateTime)
    {
        var yearBytes = BitConverter.GetBytes((ushort)dateTime.Year);

        return
        [
            yearBytes[0],
            yearBytes[1],
            (byte)dateTime.Month,
            (byte)dateTime.Day,
            (byte)dateTime.Hour,
            (byte)dateTime.Minute,
            (byte)dateTime.Second,
            // The week starts on monday on the rust application
            dateTime.DayOfWeek == DayOfWeek.Sunday ? (byte)7 : (byte)dateTime.DayOfWeek
        ];
    }
}