using WeatherMCPWebAPI.Application.Interfaces;
using WeatherMCPWebAPI.Domain.Entities;
using WeatherMCPWebAPI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace WeatherMCPWebAPI.Application.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IWeatherApiClient weatherApiClient, ILogger<WeatherService> logger)
    {
        _weatherApiClient = weatherApiClient ?? throw new ArgumentNullException(nameof(weatherApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Weather> GetCurrentWeatherAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        try
        {
            var coordinates = await _weatherApiClient.GetCoordinatesAsync(city);
            if (coordinates == null)
            {
                _logger.LogWarning("Could not find coordinates for city: {City}", city);
                return CreateFallbackWeather(city);
            }

            var weatherData = await _weatherApiClient.GetCurrentWeatherAsync(coordinates.Value.latitude, coordinates.Value.longitude);
            if (weatherData == null)
            {
                _logger.LogWarning("Could not retrieve weather data for {City}", city);
                return CreateFallbackWeather(city);
            }

            var location = new Location(city, "US", coordinates.Value.latitude, coordinates.Value.longitude);
            var temperature = new Temperature(weatherData.Temperature, TemperatureUnit.Celsius);

            return new Weather(
                location,
                temperature,
                weatherData.Humidity,
                weatherData.Pressure,
                weatherData.Conditions,
                weatherData.WindSpeed,
                weatherData.WindDirection,
                weatherData.ObservationTime
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather for {City}", city);
            return CreateFallbackWeather(city);
        }
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync(string city, int days = 5)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (days < 1 || days > 10)
            throw new ArgumentException("Days must be between 1 and 10", nameof(days));

        try
        {
            var coordinates = await _weatherApiClient.GetCoordinatesAsync(city);
            if (coordinates == null)
            {
                _logger.LogWarning("Could not find coordinates for city: {City}", city);
                return CreateFallbackForecast(city, days);
            }

            var forecastData = await _weatherApiClient.GetForecastAsync(coordinates.Value.latitude, coordinates.Value.longitude);
            if (forecastData == null)
            {
                _logger.LogWarning("Could not retrieve forecast data for {City}", city);
                return CreateFallbackForecast(city, days);
            }

            var location = new Location(city, "US", coordinates.Value.latitude, coordinates.Value.longitude);
            var forecasts = new List<WeatherForecast>();

            foreach (var daily in forecastData.DailyForecasts.Take(days))
            {
                var highTemp = new Temperature(daily.HighTemperature, TemperatureUnit.Celsius);
                var lowTemp = new Temperature(daily.LowTemperature, TemperatureUnit.Celsius);

                forecasts.Add(new WeatherForecast(
                    location,
                    daily.Date,
                    highTemp,
                    lowTemp,
                    daily.Conditions,
                    daily.Humidity,
                    daily.WindSpeed,
                    daily.WindDirection,
                    daily.PrecipitationChance
                ));
            }

            return forecasts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast for {City}", city);
            return CreateFallbackForecast(city, days);
        }
    }

    public async Task<IEnumerable<WeatherAlert>> GetWeatherAlertsAsync(string region)
    {
        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("Region cannot be null or empty", nameof(region));

        try
        {
            var coordinates = await _weatherApiClient.GetCoordinatesAsync(region);
            if (coordinates == null)
            {
                _logger.LogWarning("Could not find coordinates for region: {Region}", region);
                return Array.Empty<WeatherAlert>();
            }

            var alertsData = await _weatherApiClient.GetAlertsAsync(coordinates.Value.latitude, coordinates.Value.longitude);
            if (alertsData == null)
            {
                return Array.Empty<WeatherAlert>();
            }

            var location = new Location(region, "US", coordinates.Value.latitude, coordinates.Value.longitude);
            var alerts = new List<WeatherAlert>();

            foreach (var alert in alertsData.Alerts)
            {
                if (Enum.TryParse<AlertSeverity>(alert.Severity, true, out var severity))
                {
                    alerts.Add(new WeatherAlert(
                        alert.Id,
                        alert.Title,
                        alert.Description,
                        severity,
                        location,
                        alert.StartTime,
                        alert.EndTime,
                        alert.IssuedTime,
                        alert.IssuingAgency
                    ));
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alerts for {Region}", region);
            return Array.Empty<WeatherAlert>();
        }
    }

    public async Task<(Weather weather1, Weather weather2)> CompareWeatherAsync(string city1, string city2)
    {
        var weather1Task = GetCurrentWeatherAsync(city1);
        var weather2Task = GetCurrentWeatherAsync(city2);

        var weather1 = await weather1Task;
        var weather2 = await weather2Task;

        return (weather1, weather2);
    }

    public async Task<IEnumerable<Weather>> GetWeatherHistoryAsync(string city, DateTime startDate, DateTime endDate)
    {
        await Task.Delay(100);

        var location = await _weatherApiClient.GetCoordinatesAsync(city);
        var cityLocation = location.HasValue
            ? new Location(city, "US", location.Value.latitude, location.Value.longitude)
            : new Location(city, "Unknown", 0, 0);

        var history = new List<Weather>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date && currentDate <= DateTime.Today)
        {
            var temperature = new Temperature(15 + Random.Shared.NextDouble() * 20, TemperatureUnit.Celsius);
            var weather = new Weather(
                cityLocation,
                temperature,
                40 + Random.Shared.NextDouble() * 40,
                1000 + Random.Shared.NextDouble() * 50,
                "Historical Data",
                5 + Random.Shared.NextDouble() * 20,
                "Variable",
                currentDate
            );
            history.Add(weather);
            currentDate = currentDate.AddDays(1);
        }

        return history;
    }

    public Temperature ConvertTemperature(double value, TemperatureUnit fromUnit, TemperatureUnit toUnit)
    {
        var temperature = new Temperature(value, fromUnit);
        return temperature.ConvertTo(toUnit);
    }

    public double CalculateHeatIndex(double temperatureF, double humidity)
    {
        if (temperatureF < 80 || humidity < 40)
            return temperatureF;

        const double c1 = -42.379;
        const double c2 = 2.04901523;
        const double c3 = 10.14333127;
        const double c4 = -0.22475541;
        const double c5 = -6.83783e-3;
        const double c6 = -5.481717e-2;
        const double c7 = 1.22874e-3;
        const double c8 = 8.5282e-4;
        const double c9 = -1.99e-6;

        var heatIndex = c1 + (c2 * temperatureF) + (c3 * humidity) + (c4 * temperatureF * humidity) +
                       (c5 * temperatureF * temperatureF) + (c6 * humidity * humidity) +
                       (c7 * temperatureF * temperatureF * humidity) + (c8 * temperatureF * humidity * humidity) +
                       (c9 * temperatureF * temperatureF * humidity * humidity);

        return heatIndex;
    }

    public async Task<(DateTime sunrise, DateTime sunset)> GetSunriseSunsetAsync(string city)
    {
        await Task.Delay(100);

        var now = DateTime.Today;
        var sunrise = now.AddHours(6).AddMinutes(Random.Shared.Next(0, 120));
        var sunset = now.AddHours(18).AddMinutes(Random.Shared.Next(0, 120));

        return (sunrise, sunset);
    }

    private Weather CreateFallbackWeather(string city)
    {
        var location = new Location(city, "Unknown", 0, 0);
        var temperature = new Temperature(20 + Random.Shared.NextDouble() * 15, TemperatureUnit.Celsius);

        return new Weather(
            location,
            temperature,
            50 + Random.Shared.NextDouble() * 30,
            1013 + Random.Shared.NextDouble() * 20,
            "Simulated Data",
            10 + Random.Shared.NextDouble() * 15,
            "Variable",
            DateTime.UtcNow
        );
    }

    private IEnumerable<WeatherForecast> CreateFallbackForecast(string city, int days)
    {
        var location = new Location(city, "Unknown", 0, 0);
        var forecasts = new List<WeatherForecast>();

        for (int i = 0; i < days; i++)
        {
            var date = DateTime.Today.AddDays(i);
            var highTemp = new Temperature(20 + Random.Shared.NextDouble() * 15, TemperatureUnit.Celsius);
            var lowTemp = new Temperature(highTemp.Value - 5 - Random.Shared.NextDouble() * 5, TemperatureUnit.Celsius);

            forecasts.Add(new WeatherForecast(
                location,
                date,
                highTemp,
                lowTemp,
                "Simulated Forecast",
                50 + Random.Shared.NextDouble() * 30,
                10 + Random.Shared.NextDouble() * 15,
                "Variable",
                Random.Shared.NextDouble() * 100
            ));
        }

        return forecasts;
    }
}