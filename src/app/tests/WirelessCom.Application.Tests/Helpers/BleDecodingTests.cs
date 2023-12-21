using WirelessCom.Application.Helpers;

namespace WirelessCom.Application.Tests.Helpers;

[TestFixture]
public class BleDecodingTests
{
    [Test]
    public void BleDecoding_BleBytesToInt_ThrowsArgumentException_WhenByteArrayLengthIsNotTwo()
    {
        var bytes = new byte[] { 0x00, 0x00, 0x00 };
        var exception = Assert.Throws<ArgumentException>(() => BleDecoding.BleBytesToInt(bytes));

        Assert.That(exception!.Message, Is.EqualTo("Byte array must be of length 2 (Parameter 'bytes')"));
    }
    
    [TestCase(new byte[] { 252, 8 }, 23)]
    [TestCase(new byte[] { 170, 20 }, 52.9)]
    public void BleDecoding_BleBytesToInt_ReturnsCorrectValue_WhenByteArrayLengthIsTwo(byte[] bytes, decimal expected)
    {
        Assert.That(BleDecoding.BleBytesToInt(bytes), Is.EqualTo(expected));
    }
}