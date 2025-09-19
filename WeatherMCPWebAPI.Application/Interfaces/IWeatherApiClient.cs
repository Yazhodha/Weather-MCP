using WeatherMCPWebAPI.Application.Models;

namespace WeatherMCPWebAPI.Application.Interfaces;

public interface IWeatherApiClient
{
    Task<WeatherApiResponse?> GetCurrentWeatherAsync(double latitude, double longitude);
    Task<ForecastApiResponse?> GetForecastAsync(double latitude, double longitude);
    Task<AlertsApiResponse?> GetAlertsAsync(double latitude, double longitude);
    Task<(double latitude, double longitude)?> GetCoordinatesAsync(string city);
}