namespace WeatherMCPWebAPI.Domain.ValueObjects;

public record Temperature
{
    public double Value { get; init; }
    public TemperatureUnit Unit { get; init; }

    public Temperature(double value, TemperatureUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public Temperature ConvertTo(TemperatureUnit targetUnit)
    {
        if (Unit == targetUnit)
            return this;

        return targetUnit switch
        {
            TemperatureUnit.Celsius => Unit switch
            {
                TemperatureUnit.Fahrenheit => new Temperature((Value - 32) * 5 / 9, TemperatureUnit.Celsius),
                TemperatureUnit.Kelvin => new Temperature(Value - 273.15, TemperatureUnit.Celsius),
                _ => this
            },
            TemperatureUnit.Fahrenheit => Unit switch
            {
                TemperatureUnit.Celsius => new Temperature(Value * 9 / 5 + 32, TemperatureUnit.Fahrenheit),
                TemperatureUnit.Kelvin => new Temperature((Value - 273.15) * 9 / 5 + 32, TemperatureUnit.Fahrenheit),
                _ => this
            },
            TemperatureUnit.Kelvin => Unit switch
            {
                TemperatureUnit.Celsius => new Temperature(Value + 273.15, TemperatureUnit.Kelvin),
                TemperatureUnit.Fahrenheit => new Temperature((Value - 32) * 5 / 9 + 273.15, TemperatureUnit.Kelvin),
                _ => this
            },
            _ => this
        };
    }

    public override string ToString()
    {
        var unitSymbol = Unit switch
        {
            TemperatureUnit.Celsius => "°C",
            TemperatureUnit.Fahrenheit => "°F",
            TemperatureUnit.Kelvin => "K",
            _ => ""
        };
        return $"{Value:F1}{unitSymbol}";
    }
}

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit,
    Kelvin
}