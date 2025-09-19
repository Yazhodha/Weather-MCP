using WeatherMCPWebAPI.Domain.ValueObjects;

namespace WeatherMCPWebAPI.Domain.Entities;

public class WeatherForecast
{
    public Location Location { get; init; }
    public DateTime Date { get; init; }
    public Temperature HighTemperature { get; init; }
    public Temperature LowTemperature { get; init; }
    public string Conditions { get; init; }
    public double Humidity { get; init; }
    public double WindSpeed { get; init; }
    public string WindDirection { get; init; }
    public double PrecipitationChance { get; init; }

    public WeatherForecast(Location location, DateTime date, Temperature highTemperature,
                          Temperature lowTemperature, string conditions, double humidity,
                          double windSpeed, string windDirection, double precipitationChance)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Date = date;
        HighTemperature = highTemperature ?? throw new ArgumentNullException(nameof(highTemperature));
        LowTemperature = lowTemperature ?? throw new ArgumentNullException(nameof(lowTemperature));

        if (humidity < 0 || humidity > 100)
            throw new ArgumentException("Humidity must be between 0 and 100", nameof(humidity));

        if (windSpeed < 0)
            throw new ArgumentException("Wind speed cannot be negative", nameof(windSpeed));

        if (precipitationChance < 0 || precipitationChance > 100)
            throw new ArgumentException("Precipitation chance must be between 0 and 100", nameof(precipitationChance));

        Conditions = conditions ?? string.Empty;
        Humidity = humidity;
        WindSpeed = windSpeed;
        WindDirection = windDirection ?? string.Empty;
        PrecipitationChance = precipitationChance;
    }

    public string GetFormattedSummary()
    {
        var highC = HighTemperature.ConvertTo(TemperatureUnit.Celsius);
        var lowC = LowTemperature.ConvertTo(TemperatureUnit.Celsius);
        var highF = HighTemperature.ConvertTo(TemperatureUnit.Fahrenheit);
        var lowF = LowTemperature.ConvertTo(TemperatureUnit.Fahrenheit);

        return $"📅 {Date:dddd, MMMM dd} - {Location}\n" +
               $"🌡️  High: {highC} ({highF}), Low: {lowC} ({lowF})\n" +
               $"☁️  {Conditions}\n" +
               $"💧 Humidity: {Humidity:F0}%\n" +
               $"🌬️  Wind: {WindSpeed:F0} km/h {WindDirection}\n" +
               $"🌧️  Precipitation: {PrecipitationChance:F0}%";
    }
}