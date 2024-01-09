using WirelessCom.Application.Helpers;

namespace WirelessCom.Application.Tests.Helpers;

[TestFixture]
public class BleDecodingTests
{
    [Test]
    public void BleDecoding_BleBytesToDouble_ThrowsArgumentException_WhenByteArrayLengthIsNotTwo()
    {
        var bytes = new byte[] { 0x00, 0x00, 0x00 };
        var exception = Assert.Throws<ArgumentException>(() => BleDecoding.BleBytesToDouble(bytes));

        Assert.That(exception!.Message, Is.EqualTo("Byte array must be of length 2 (Parameter 'bytes')"));
    }

    [TestCase(new byte[] { 252, 8 }, 23)]
    [TestCase(new byte[] { 170, 20 }, 52.9)]
    public void BleDecoding_BleBytesToDouble_ReturnsCorrectValue_WhenByteArrayLengthIsTwo(byte[] bytes, decimal expected)
    {
        Assert.That(BleDecoding.BleBytesToDouble(bytes), Is.EqualTo(expected));
    }

    [Test]
    public void BleDecoding_BleBytesToDateTime_ThrowsArgumentException_WhenByteArrayLengthIsLessThanSix()
    {
        var bytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };
        var exception = Assert.Throws<ArgumentException>(() => BleDecoding.BleBytesToDateTime(bytes));

        Assert.That(exception!.Message, Is.EqualTo("Byte array must a greater length of 6 (Parameter 'readingBytes')"));
    }

    [Test]
    public void BleDecoding_BleBytesToDateTime_ReturnsCorrectValue()
    {
        var bytes = new byte[]
        {
            232, 7, 1, 6, 17, 5, 57
        };

        var expected = new DateTime(
            2024,
            1,
            6,
            18,
            5,
            57
        );

        Assert.That(BleDecoding.BleBytesToDateTime(bytes), Is.EqualTo(expected));
    }
}