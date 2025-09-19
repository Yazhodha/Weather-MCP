using WeatherMCPWebAPI.Domain.ValueObjects;

namespace WeatherMCPWebAPI.Domain.Entities;

public class WeatherAlert
{
    public string Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public AlertSeverity Severity { get; init; }
    public Location Location { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public DateTime IssuedTime { get; init; }
    public string IssuingAgency { get; init; }

    public WeatherAlert(string id, string title, string description, AlertSeverity severity,
                       Location location, DateTime startTime, DateTime endTime,
                       DateTime issuedTime, string issuingAgency)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Alert ID cannot be null or empty", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Alert title cannot be null or empty", nameof(title));

        Id = id;
        Title = title;
        Description = description ?? string.Empty;
        Severity = severity;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        StartTime = startTime;
        EndTime = endTime;
        IssuedTime = issuedTime;
        IssuingAgency = issuingAgency ?? string.Empty;
    }

    public bool IsActive => DateTime.UtcNow >= StartTime && DateTime.UtcNow <= EndTime;

    public string GetFormattedSummary()
    {
        var severityIcon = Severity switch
        {
            AlertSeverity.Minor => "⚠️",
            AlertSeverity.Moderate => "⚠️",
            AlertSeverity.Severe => "🚨",
            AlertSeverity.Extreme => "🔴",
            _ => "ℹ️"
        };

        var status = IsActive ? "ACTIVE" : "INACTIVE";

        return $"{severityIcon} {Severity.ToString().ToUpper()} ALERT - {status}\n" +
               $"📍 {Location}\n" +
               $"🏷️  {Title}\n" +
               $"📝 {Description}\n" +
               $"⏰ {StartTime:yyyy-MM-dd HH:mm} - {EndTime:yyyy-MM-dd HH:mm} UTC\n" +
               $"🏢 Issued by: {IssuingAgency}";
    }
}

public enum AlertSeverity
{
    Minor,
    Moderate,
    Severe,
    Extreme
}