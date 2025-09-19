namespace WeatherMCPWebAPI.Domain.ValueObjects;

public record Location
{
    public string City { get; init; }
    public string Country { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public Location(string city, string country, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        City = city;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString()
    {
        return $"{City}, {Country}";
    }

    public string ToCoordinateString()
    {
        return $"{Latitude:F4},{Longitude:F4}";
    }
}