namespace WeatherMCPWebAPI.Application.Models;

public class WeatherApiResponse
{
    public DateTime ObservationTime { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public double WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
}

public class ForecastApiResponse
{
    public List<DailyForecast> DailyForecasts { get; set; } = new();
}

public class DailyForecast
{
    public DateTime Date { get; set; }
    public double HighTemperature { get; set; }
    public double LowTemperature { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public double PrecipitationChance { get; set; }
}

public class AlertsApiResponse
{
    public List<AlertInfo> Alerts { get; set; } = new();
}

public class AlertInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime IssuedTime { get; set; }
    public string IssuingAgency { get; set; } = string.Empty;
}