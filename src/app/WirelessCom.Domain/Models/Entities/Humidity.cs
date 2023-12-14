namespace WirelessCom.Domain.Models.Entities;

public record Humidity(double Value, DateTime DateTime, string PeripheralName) : BaseEntity
{
    public override string ToString()
    {
        return $"{Value} %";
    }
}