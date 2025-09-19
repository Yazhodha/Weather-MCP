using ModelContextProtocol;
using System.ComponentModel;
using ModelContextProtocol.Server;
using WeatherMCPWebAPI.Application.Interfaces;
using WeatherMCPWebAPI.Domain.ValueObjects;

namespace WeatherMCPWebAPI.Tools;

[McpServerToolType]
public class WeatherTools
{
    private readonly IWeatherService _weatherService;

    public WeatherTools(IWeatherService weatherService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
    }

    [McpServerTool]
    [Description("Get current weather conditions for a specified city. Returns temperature, humidity, pressure, wind conditions, and more.")]
    public async Task<string> GetCurrentWeather(
        [Description("The name of the city to get weather for (e.g., 'New York', 'London', 'Tokyo')")] string city)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city))
                return "❌ Error: City name cannot be empty.";

            var weather = await _weatherService.GetCurrentWeatherAsync(city);
            return weather.GetFormattedSummary();
        }
        catch (Exception ex)
        {
            return $"❌ Error retrieving current weather for {city}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get weather forecast for the next 1-5 days for a specified city. Includes high/low temperatures, conditions, and precipitation chances.")]
    public async Task<string> GetWeatherForecast(
        [Description("The name of the city to get forecast for")] string city,
        [Description("Number of days to forecast (1-5, default: 5)")] int days = 5)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city))
                return "❌ Error: City name cannot be empty.";

            if (days < 1 || days > 5)
                return "❌ Error: Days must be between 1 and 5.";

            var forecasts = await _weatherService.GetWeatherForecastAsync(city, days);
            var result = $"🌦️  {days}-Day Weather Forecast for {city}\n" +
                        "═══════════════════════════════════════\n\n";

            foreach (var forecast in forecasts)
            {
                result += forecast.GetFormattedSummary() + "\n\n";
            }

            return result.TrimEnd();
        }
        catch (Exception ex)
        {
            return $"❌ Error retrieving weather forecast for {city}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get active weather alerts and warnings for a specified region or city.")]
    public async Task<string> GetWeatherAlerts(
        [Description("The name of the region or city to check for alerts")] string region)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(region))
                return "❌ Error: Region name cannot be empty.";

            var alerts = await _weatherService.GetWeatherAlertsAsync(region);

            if (!alerts.Any())
                return $"✅ No active weather alerts for {region}.";

            var result = $"⚠️  Active Weather Alerts for {region}\n" +
                        "═══════════════════════════════════════\n\n";

            foreach (var alert in alerts)
            {
                result += alert.GetFormattedSummary() + "\n\n";
            }

            return result.TrimEnd();
        }
        catch (Exception ex)
        {
            return $"❌ Error retrieving weather alerts for {region}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Compare current weather conditions between two cities side by side.")]
    public async Task<string> CompareWeather(
        [Description("The first city to compare")] string city1,
        [Description("The second city to compare")] string city2)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city1) || string.IsNullOrWhiteSpace(city2))
                return "❌ Error: Both city names must be provided.";

            var (weather1, weather2) = await _weatherService.CompareWeatherAsync(city1, city2);

            var tempC1 = weather1.Temperature.ConvertTo(TemperatureUnit.Celsius);
            var tempF1 = weather1.Temperature.ConvertTo(TemperatureUnit.Fahrenheit);
            var tempC2 = weather2.Temperature.ConvertTo(TemperatureUnit.Celsius);
            var tempF2 = weather2.Temperature.ConvertTo(TemperatureUnit.Fahrenheit);

            return $"🔍 Weather Comparison\n" +
                  "═══════════════════════════════════════\n\n" +
                  $"📍 {weather1.Location} vs {weather2.Location}\n\n" +
                  $"🌡️  Temperature:\n" +
                  $"   • {city1}: {tempC1} ({tempF1})\n" +
                  $"   • {city2}: {tempC2} ({tempF2})\n" +
                  $"   • Difference: {Math.Abs(tempC1.Value - tempC2.Value):F1}°C\n\n" +
                  $"💧 Humidity:\n" +
                  $"   • {city1}: {weather1.Humidity:F0}%\n" +
                  $"   • {city2}: {weather2.Humidity:F0}%\n\n" +
                  $"🌬️  Wind Speed:\n" +
                  $"   • {city1}: {weather1.WindSpeed:F0} km/h {weather1.WindDirection}\n" +
                  $"   • {city2}: {weather2.WindSpeed:F0} km/h {weather2.WindDirection}\n\n" +
                  $"☁️  Conditions:\n" +
                  $"   • {city1}: {weather1.Conditions}\n" +
                  $"   • {city2}: {weather2.Conditions}";
        }
        catch (Exception ex)
        {
            return $"❌ Error comparing weather between {city1} and {city2}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Get historical weather data for a city within a specified date range (simulated data for demonstration).")]
    public async Task<string> GetWeatherHistory(
        [Description("The name of the city")] string city,
        [Description("Start date in YYYY-MM-DD format")] string startDate,
        [Description("End date in YYYY-MM-DD format")] string endDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city))
                return "❌ Error: City name cannot be empty.";

            if (!DateTime.TryParse(startDate, out var start))
                return "❌ Error: Invalid start date format. Use YYYY-MM-DD.";

            if (!DateTime.TryParse(endDate, out var end))
                return "❌ Error: Invalid end date format. Use YYYY-MM-DD.";

            if (start > end)
                return "❌ Error: Start date cannot be after end date.";

            if (end > DateTime.Today)
                return "❌ Error: End date cannot be in the future.";

            var history = await _weatherService.GetWeatherHistoryAsync(city, start, end);

            if (!history.Any())
                return $"📊 No historical weather data available for {city} in the specified period.";

            var result = $"📊 Historical Weather for {city}\n" +
                        $"📅 {start:yyyy-MM-dd} to {end:yyyy-MM-dd}\n" +
                        "═══════════════════════════════════════\n\n";

            foreach (var weather in history.Take(10))
            {
                var tempC = weather.Temperature.ConvertTo(TemperatureUnit.Celsius);
                var tempF = weather.Temperature.ConvertTo(TemperatureUnit.Fahrenheit);
                result += $"📅 {weather.ObservationTime:yyyy-MM-dd}: {tempC} ({tempF}), {weather.Conditions}, {weather.Humidity:F0}% humidity\n";
            }

            if (history.Count() > 10)
            {
                result += $"\n... and {history.Count() - 10} more days";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"❌ Error retrieving weather history for {city}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Convert temperature between Celsius, Fahrenheit, and Kelvin units.")]
    public Task<string> ConvertTemperature(
        [Description("The temperature value to convert")] double value,
        [Description("Source temperature unit (Celsius, Fahrenheit, or Kelvin)")] string fromUnit,
        [Description("Target temperature unit (Celsius, Fahrenheit, or Kelvin)")] string toUnit)
    {
        try
        {
            if (!Enum.TryParse<TemperatureUnit>(fromUnit, true, out var from))
                return Task.FromResult("❌ Error: Invalid source unit. Use Celsius, Fahrenheit, or Kelvin.");

            if (!Enum.TryParse<TemperatureUnit>(toUnit, true, out var to))
                return Task.FromResult("❌ Error: Invalid target unit. Use Celsius, Fahrenheit, or Kelvin.");

            var converted = _weatherService.ConvertTemperature(value, from, to);

            var fromSymbol = from switch
            {
                TemperatureUnit.Celsius => "°C",
                TemperatureUnit.Fahrenheit => "°F",
                TemperatureUnit.Kelvin => "K",
                _ => ""
            };

            var toSymbol = to switch
            {
                TemperatureUnit.Celsius => "°C",
                TemperatureUnit.Fahrenheit => "°F",
                TemperatureUnit.Kelvin => "K",
                _ => ""
            };

            return Task.FromResult($"🌡️  Temperature Conversion\n" +
                                 "═══════════════════════════════════════\n" +
                                 $"📊 {value:F1}{fromSymbol} = {converted.Value:F1}{toSymbol}");
        }
        catch (Exception ex)
        {
            return Task.FromResult($"❌ Error converting temperature: {ex.Message}");
        }
    }

    [McpServerTool]
    [Description("Calculate the heat index (apparent temperature) from temperature and humidity values.")]
    public Task<string> CalculateHeatIndex(
        [Description("Temperature in Fahrenheit")] double temperatureF,
        [Description("Relative humidity percentage (0-100)")] double humidity)
    {
        try
        {
            if (humidity < 0 || humidity > 100)
                return Task.FromResult("❌ Error: Humidity must be between 0 and 100.");

            var heatIndex = _weatherService.CalculateHeatIndex(temperatureF, humidity);
            var tempC = new Temperature(temperatureF, TemperatureUnit.Fahrenheit).ConvertTo(TemperatureUnit.Celsius);
            var heatIndexC = new Temperature(heatIndex, TemperatureUnit.Fahrenheit).ConvertTo(TemperatureUnit.Celsius);

            var warning = "";
            if (heatIndex >= 105)
                warning = "\n⚠️  EXTREME DANGER: Heat stroke imminent!";
            else if (heatIndex >= 90)
                warning = "\n⚠️  DANGER: Heat exhaustion and heat cramps likely!";
            else if (heatIndex >= 80)
                warning = "\n⚠️  CAUTION: Fatigue possible with prolonged exposure!";

            return Task.FromResult($"🔥 Heat Index Calculation\n" +
                                 "═══════════════════════════════════════\n" +
                                 $"🌡️  Temperature: {tempC} ({temperatureF:F1}°F)\n" +
                                 $"💧 Humidity: {humidity:F0}%\n" +
                                 $"🔥 Heat Index: {heatIndexC} ({heatIndex:F1}°F)\n" +
                                 $"📊 Feels like: {Math.Abs(heatIndex - temperatureF):F1}°F {(heatIndex > temperatureF ? "hotter" : "cooler")} than actual temperature" +
                                 warning);
        }
        catch (Exception ex)
        {
            return Task.FromResult($"❌ Error calculating heat index: {ex.Message}");
        }
    }

    [McpServerTool]
    [Description("Get sunrise and sunset times for a specified city (simulated data for demonstration).")]
    public async Task<string> GetSunriseSunset(
        [Description("The name of the city")] string city)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city))
                return "❌ Error: City name cannot be empty.";

            var (sunrise, sunset) = await _weatherService.GetSunriseSunsetAsync(city);
            var dayLength = sunset - sunrise;

            return $"🌅 Sunrise & Sunset for {city}\n" +
                  "═══════════════════════════════════════\n" +
                  $"📅 Date: {DateTime.Today:dddd, MMMM dd, yyyy}\n" +
                  $"🌅 Sunrise: {sunrise:HH:mm} local time\n" +
                  $"🌇 Sunset: {sunset:HH:mm} local time\n" +
                  $"⏱️  Daylight: {dayLength.Hours}h {dayLength.Minutes}m\n\n" +
                  "⚠️  Note: This is simulated data for demonstration purposes.";
        }
        catch (Exception ex)
        {
            return $"❌ Error retrieving sunrise/sunset times for {city}: {ex.Message}";
        }
    }
}