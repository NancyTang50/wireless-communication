using WirelessCom.Application.Helpers;

namespace WirelessCom.Application.Tests.Helpers;

[TestFixture]
public class BleEncodingTests
{
    [Test]
    public void BleEncoding_ShouldEncodeDateTimeBytes()
    {
        var expected = new byte[]
        {
            232, 7, 1, 6, 22, 5, 57, 6
        };

        var dateTime = new DateTime(
            2024,
            1,
            6,
            22,
            5,
            57
        );

        var result = BleEncoding.GetDateTimeBleBytes(dateTime);
        result.Should().BeEquivalentTo(expected);
    }
}