using WirelessCom.Application.Extensions;

namespace WirelessCom.Application.Tests.Extensions;

public class IntExtensionsTests
{
    [Test]
    public void ToBleGuid_should_convert_int_to_BLE_guid()
    {
        // Arrange
        const int value = 0x181A;

        // Act
        var result = value.ToBleGuid();

        // Assert
        result.ToString().Should().Be("0000181a-0000-1000-8000-00805f9b34fb");
    }
}