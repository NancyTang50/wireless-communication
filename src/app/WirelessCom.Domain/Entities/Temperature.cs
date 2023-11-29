namespace WirelessCom.Domain.Entities;

public record Temperature(double Kelvin, DateTime DateTime, string PeripheralName) : BaseEntity
{
    public double Fahrenheit => Celsius * 9 / 5 + 32;
    public double Celsius => Kelvin - 273.15;

    public double ToPreferredUnit(TemperatureUnit temperatureUnit)
    {
        return temperatureUnit switch
        {
            TemperatureUnit.Celsius => Celsius,
            TemperatureUnit.Kelvin => Kelvin,
            TemperatureUnit.Fahrenheit => Fahrenheit,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string ToString(TemperatureUnit temperatureUnit)
    {
        return temperatureUnit switch
        {
            TemperatureUnit.Celsius => $"{Celsius} °C",
            TemperatureUnit.Kelvin => $"{Kelvin} K",
            TemperatureUnit.Fahrenheit => $"{Fahrenheit} °F",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}