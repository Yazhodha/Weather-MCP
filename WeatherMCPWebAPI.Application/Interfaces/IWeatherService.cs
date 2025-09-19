using WeatherMCPWebAPI.Domain.Entities;
using WeatherMCPWebAPI.Domain.ValueObjects;

namespace WeatherMCPWebAPI.Application.Interfaces;

public interface IWeatherService
{
    Task<Weather> GetCurrentWeatherAsync(string city);
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync(string city, int days = 5);
    Task<IEnumerable<WeatherAlert>> GetWeatherAlertsAsync(string region);
    Task<(Weather weather1, Weather weather2)> CompareWeatherAsync(string city1, string city2);
    Task<IEnumerable<Weather>> GetWeatherHistoryAsync(string city, DateTime startDate, DateTime endDate);
    Temperature ConvertTemperature(double value, TemperatureUnit fromUnit, TemperatureUnit toUnit);
    double CalculateHeatIndex(double temperatureF, double humidity);
    Task<(DateTime sunrise, DateTime sunset)> GetSunriseSunsetAsync(string city);
}