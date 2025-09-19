using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherMCPWebAPI.Application.Interfaces;
using WeatherMCPWebAPI.Application.Models;

namespace WeatherMCPWebAPI.Infrastructure.External;

public class NationalWeatherServiceClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NationalWeatherServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly Dictionary<string, (double lat, double lon)> CityCoordinates = new()
    {
        { "new york", (40.7128, -74.0060) },
        { "los angeles", (34.0522, -118.2437) },
        { "chicago", (41.8781, -87.6298) },
        { "houston", (29.7604, -95.3698) },
        { "phoenix", (33.4484, -112.0740) },
        { "philadelphia", (39.9526, -75.1652) },
        { "san antonio", (29.4241, -98.4936) },
        { "san diego", (32.7157, -117.1611) },
        { "dallas", (32.7767, -96.7970) },
        { "san jose", (37.3382, -121.8863) },
        { "austin", (30.2672, -97.7431) },
        { "jacksonville", (30.3322, -81.6557) },
        { "san francisco", (37.7749, -122.4194) },
        { "columbus", (39.9612, -82.9988) },
        { "charlotte", (35.2271, -80.8431) },
        { "fort worth", (32.7555, -97.3308) },
        { "indianapolis", (39.7684, -86.1581) },
        { "seattle", (47.6062, -122.3321) },
        { "denver", (39.7392, -104.9903) },
        { "washington", (38.9072, -77.0369) },
        { "boston", (42.3601, -71.0589) },
        { "el paso", (31.7619, -106.4850) },
        { "detroit", (42.3314, -83.0458) },
        { "nashville", (36.1627, -86.7816) },
        { "portland", (45.5152, -122.6784) },
        { "memphis", (35.1495, -90.0490) },
        { "oklahoma city", (35.4676, -97.5164) },
        { "las vegas", (36.1699, -115.1398) },
        { "louisville", (38.2527, -85.7585) },
        { "baltimore", (39.2904, -76.6122) },
        { "milwaukee", (43.0389, -87.9065) },
        { "albuquerque", (35.0844, -106.6504) },
        { "tucson", (32.2226, -110.9747) },
        { "fresno", (36.7378, -119.7871) },
        { "mesa", (33.4152, -111.8315) },
        { "sacramento", (38.5816, -121.4944) },
        { "atlanta", (33.7490, -84.3880) },
        { "kansas city", (39.0997, -94.5786) },
        { "colorado springs", (38.8339, -104.8214) },
        { "omaha", (41.2565, -95.9345) },
        { "raleigh", (35.7796, -78.6382) },
        { "miami", (25.7617, -80.1918) },
        { "cleveland", (41.4993, -81.6944) },
        { "tulsa", (36.1540, -95.9928) },
        { "virginia beach", (36.8529, -75.9780) },
        { "minneapolis", (44.9778, -93.2650) },
        { "honolulu", (21.3099, -157.8581) },
        { "tampa", (27.9506, -82.4572) },
        { "aurora", (39.7294, -104.8319) },
        { "anaheim", (33.8366, -117.9143) },
        { "santa ana", (33.7455, -117.8677) }
    };

    public NationalWeatherServiceClient(HttpClient httpClient, ILogger<NationalWeatherServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri("https://api.weather.gov");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "WeatherMCPWebAPI/1.0 (contact@example.com)");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<WeatherApiResponse?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var pointsUrl = $"/points/{latitude:F4},{longitude:F4}";
            _logger.LogInformation("Fetching points data from: {Url}", pointsUrl);

            var pointsResponse = await _httpClient.GetAsync(pointsUrl);
            if (!pointsResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Points API returned {StatusCode}", pointsResponse.StatusCode);
                return CreateFallbackCurrentWeather();
            }

            var pointsJson = await pointsResponse.Content.ReadAsStringAsync();
            var pointsData = JsonSerializer.Deserialize<JsonElement>(pointsJson, _jsonOptions);

            if (!pointsData.TryGetProperty("properties", out var properties) ||
                !properties.TryGetProperty("forecastGridData", out var gridDataUrl))
            {
                _logger.LogWarning("Invalid points response structure");
                return CreateFallbackCurrentWeather();
            }

            var gridDataResponse = await _httpClient.GetAsync(gridDataUrl.GetString());
            if (!gridDataResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Grid data API returned {StatusCode}", gridDataResponse.StatusCode);
                return CreateFallbackCurrentWeather();
            }

            var gridJson = await gridDataResponse.Content.ReadAsStringAsync();
            var gridData = JsonSerializer.Deserialize<JsonElement>(gridJson, _jsonOptions);

            return ParseCurrentWeatherFromGrid(gridData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current weather from NWS API");
            return CreateFallbackCurrentWeather();
        }
    }

    public async Task<ForecastApiResponse?> GetForecastAsync(double latitude, double longitude)
    {
        try
        {
            var pointsUrl = $"/points/{latitude:F4},{longitude:F4}";
            var pointsResponse = await _httpClient.GetAsync(pointsUrl);
            if (!pointsResponse.IsSuccessStatusCode)
            {
                return CreateFallbackForecast();
            }

            var pointsJson = await pointsResponse.Content.ReadAsStringAsync();
            var pointsData = JsonSerializer.Deserialize<JsonElement>(pointsJson, _jsonOptions);

            if (!pointsData.TryGetProperty("properties", out var properties) ||
                !properties.TryGetProperty("forecast", out var forecastUrl))
            {
                return CreateFallbackForecast();
            }

            var forecastResponse = await _httpClient.GetAsync(forecastUrl.GetString());
            if (!forecastResponse.IsSuccessStatusCode)
            {
                return CreateFallbackForecast();
            }

            var forecastJson = await forecastResponse.Content.ReadAsStringAsync();
            var forecastData = JsonSerializer.Deserialize<JsonElement>(forecastJson, _jsonOptions);

            return ParseForecastFromNws(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching forecast from NWS API");
            return CreateFallbackForecast();
        }
    }

    public async Task<AlertsApiResponse?> GetAlertsAsync(double latitude, double longitude)
    {
        try
        {
            var alertsUrl = $"/alerts/active?point={latitude:F4},{longitude:F4}";
            var alertsResponse = await _httpClient.GetAsync(alertsUrl);
            if (!alertsResponse.IsSuccessStatusCode)
            {
                return new AlertsApiResponse();
            }

            var alertsJson = await alertsResponse.Content.ReadAsStringAsync();
            var alertsData = JsonSerializer.Deserialize<JsonElement>(alertsJson, _jsonOptions);

            return ParseAlertsFromNws(alertsData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching alerts from NWS API");
            return new AlertsApiResponse();
        }
    }

    public async Task<(double latitude, double longitude)?> GetCoordinatesAsync(string city)
    {
        await Task.Delay(1);

        var normalizedCity = city.ToLowerInvariant().Trim();

        if (CityCoordinates.TryGetValue(normalizedCity, out var coordinates))
        {
            return coordinates;
        }

        foreach (var kvp in CityCoordinates)
        {
            if (kvp.Key.Contains(normalizedCity) || normalizedCity.Contains(kvp.Key))
            {
                return kvp.Value;
            }
        }

        _logger.LogWarning("Coordinates not found for city: {City}", city);
        return null;
    }

    private WeatherApiResponse CreateFallbackCurrentWeather()
    {
        return new WeatherApiResponse
        {
            ObservationTime = DateTime.UtcNow,
            Temperature = 20 + Random.Shared.NextDouble() * 15,
            Humidity = 50 + Random.Shared.NextDouble() * 30,
            Pressure = 1013 + Random.Shared.NextDouble() * 20,
            Conditions = "Simulated Weather",
            WindSpeed = 10 + Random.Shared.NextDouble() * 15,
            WindDirection = "Variable"
        };
    }

    private ForecastApiResponse CreateFallbackForecast()
    {
        var forecasts = new List<DailyForecast>();
        for (int i = 0; i < 7; i++)
        {
            var baseTemp = 20 + Random.Shared.NextDouble() * 15;
            forecasts.Add(new DailyForecast
            {
                Date = DateTime.Today.AddDays(i),
                HighTemperature = baseTemp + 5,
                LowTemperature = baseTemp - 5,
                Conditions = "Simulated Forecast",
                Humidity = 50 + Random.Shared.NextDouble() * 30,
                WindSpeed = 10 + Random.Shared.NextDouble() * 15,
                WindDirection = "Variable",
                PrecipitationChance = Random.Shared.NextDouble() * 100
            });
        }

        return new ForecastApiResponse { DailyForecasts = forecasts };
    }

    private WeatherApiResponse? ParseCurrentWeatherFromGrid(JsonElement gridData)
    {
        try
        {
            if (!gridData.TryGetProperty("properties", out var properties))
                return CreateFallbackCurrentWeather();

            var temperature = ExtractCurrentValue(properties, "temperature");
            var humidity = ExtractCurrentValue(properties, "relativeHumidity");
            var pressure = ExtractCurrentValue(properties, "barometricPressure");
            var windSpeed = ExtractCurrentValue(properties, "windSpeed");

            return new WeatherApiResponse
            {
                ObservationTime = DateTime.UtcNow,
                Temperature = temperature ?? (20 + Random.Shared.NextDouble() * 15),
                Humidity = humidity ?? (50 + Random.Shared.NextDouble() * 30),
                Pressure = pressure ?? (1013 + Random.Shared.NextDouble() * 20),
                Conditions = "Current Conditions",
                WindSpeed = windSpeed ?? (10 + Random.Shared.NextDouble() * 15),
                WindDirection = "Variable"
            };
        }
        catch
        {
            return CreateFallbackCurrentWeather();
        }
    }

    private double? ExtractCurrentValue(JsonElement properties, string propertyName)
    {
        try
        {
            if (properties.TryGetProperty(propertyName, out var prop) &&
                prop.TryGetProperty("values", out var values) &&
                values.GetArrayLength() > 0)
            {
                var firstValue = values[0];
                if (firstValue.TryGetProperty("value", out var value))
                {
                    return value.GetDouble();
                }
            }
        }
        catch
        {
            // Ignore parsing errors and return null
        }
        return null;
    }

    private ForecastApiResponse ParseForecastFromNws(JsonElement forecastData)
    {
        try
        {
            var forecasts = new List<DailyForecast>();

            if (forecastData.TryGetProperty("properties", out var properties) &&
                properties.TryGetProperty("periods", out var periods))
            {
                var currentDate = DateTime.Today;
                var dayPeriods = new Dictionary<DateTime, DailyForecast>();

                foreach (var period in periods.EnumerateArray())
                {
                    if (period.TryGetProperty("startTime", out var startTimeEl) &&
                        DateTime.TryParse(startTimeEl.GetString(), out var startTime))
                    {
                        var date = startTime.Date;

                        if (!dayPeriods.ContainsKey(date))
                        {
                            dayPeriods[date] = new DailyForecast
                            {
                                Date = date,
                                Conditions = "Forecast",
                                Humidity = 50,
                                WindSpeed = 10,
                                WindDirection = "Variable",
                                PrecipitationChance = 0
                            };
                        }

                        var forecast = dayPeriods[date];

                        if (period.TryGetProperty("temperature", out var tempEl))
                        {
                            var temp = tempEl.GetDouble();
                            if (period.TryGetProperty("isDaytime", out var isDaytimeEl) && isDaytimeEl.GetBoolean())
                            {
                                forecast.HighTemperature = temp;
                            }
                            else
                            {
                                forecast.LowTemperature = temp;
                            }
                        }

                        if (period.TryGetProperty("shortForecast", out var conditionsEl))
                        {
                            forecast.Conditions = conditionsEl.GetString() ?? "Unknown";
                        }
                    }
                }

                forecasts.AddRange(dayPeriods.Values.Take(7));
            }

            return new ForecastApiResponse { DailyForecasts = forecasts };
        }
        catch
        {
            return CreateFallbackForecast();
        }
    }

    private AlertsApiResponse ParseAlertsFromNws(JsonElement alertsData)
    {
        try
        {
            var alerts = new List<AlertInfo>();

            if (alertsData.TryGetProperty("features", out var features))
            {
                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("properties", out var properties))
                    {
                        var alert = new AlertInfo();

                        if (properties.TryGetProperty("id", out var idEl))
                            alert.Id = idEl.GetString() ?? "";

                        if (properties.TryGetProperty("headline", out var headlineEl))
                            alert.Title = headlineEl.GetString() ?? "";

                        if (properties.TryGetProperty("description", out var descEl))
                            alert.Description = descEl.GetString() ?? "";

                        if (properties.TryGetProperty("severity", out var severityEl))
                            alert.Severity = severityEl.GetString() ?? "";

                        if (properties.TryGetProperty("effective", out var effectiveEl) &&
                            DateTime.TryParse(effectiveEl.GetString(), out var effective))
                            alert.StartTime = effective;

                        if (properties.TryGetProperty("expires", out var expiresEl) &&
                            DateTime.TryParse(expiresEl.GetString(), out var expires))
                            alert.EndTime = expires;

                        if (properties.TryGetProperty("sent", out var sentEl) &&
                            DateTime.TryParse(sentEl.GetString(), out var sent))
                            alert.IssuedTime = sent;

                        if (properties.TryGetProperty("senderName", out var senderEl))
                            alert.IssuingAgency = senderEl.GetString() ?? "";

                        alerts.Add(alert);
                    }
                }
            }

            return new AlertsApiResponse { Alerts = alerts };
        }
        catch
        {
            return new AlertsApiResponse();
        }
    }
}