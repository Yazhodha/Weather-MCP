using WeatherMCPWebAPI.Domain.ValueObjects;

namespace WeatherMCPWebAPI.Domain.Entities;

public class Weather
{
    public Location Location { get; init; }
    public Temperature Temperature { get; init; }
    public double Humidity { get; init; }
    public double Pressure { get; init; }
    public string Conditions { get; init; }
    public double WindSpeed { get; init; }
    public string WindDirection { get; init; }
    public DateTime ObservationTime { get; init; }

    public Weather(Location location, Temperature temperature, double humidity, double pressure,
                   string conditions, double windSpeed, string windDirection, DateTime observationTime)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Temperature = temperature ?? throw new ArgumentNullException(nameof(temperature));

        if (humidity < 0 || humidity > 100)
            throw new ArgumentException("Humidity must be between 0 and 100", nameof(humidity));

        if (pressure < 0)
            throw new ArgumentException("Pressure cannot be negative", nameof(pressure));

        if (windSpeed < 0)
            throw new ArgumentException("Wind speed cannot be negative", nameof(windSpeed));

        Humidity = humidity;
        Pressure = pressure;
        Conditions = conditions ?? string.Empty;
        WindSpeed = windSpeed;
        WindDirection = windDirection ?? string.Empty;
        ObservationTime = observationTime;
    }

    public double CalculateHeatIndex()
    {
        var tempF = Temperature.ConvertTo(TemperatureUnit.Fahrenheit).Value;

        if (tempF < 80 || Humidity < 40)
            return tempF;

        const double c1 = -42.379;
        const double c2 = 2.04901523;
        const double c3 = 10.14333127;
        const double c4 = -0.22475541;
        const double c5 = -6.83783e-3;
        const double c6 = -5.481717e-2;
        const double c7 = 1.22874e-3;
        const double c8 = 8.5282e-4;
        const double c9 = -1.99e-6;

        var t = tempF;
        var r = Humidity;

        var heatIndex = c1 + (c2 * t) + (c3 * r) + (c4 * t * r) + (c5 * t * t) +
                       (c6 * r * r) + (c7 * t * t * r) + (c8 * t * r * r) + (c9 * t * t * r * r);

        return heatIndex;
    }

    public string GetFormattedSummary()
    {
        var tempC = Temperature.ConvertTo(TemperatureUnit.Celsius);
        var tempF = Temperature.ConvertTo(TemperatureUnit.Fahrenheit);

        return $"🌤️  Current Weather for {Location}\n" +
               $"Temperature: {tempC} ({tempF})\n" +
               $"Conditions: {Conditions}\n" +
               $"Humidity: {Humidity:F0}%\n" +
               $"Pressure: {Pressure:F0} hPa\n" +
               $"Wind: {WindSpeed:F0} km/h {WindDirection}\n" +
               $"Last Updated: {ObservationTime:yyyy-MM-dd HH:mm:ss} UTC";
    }
}