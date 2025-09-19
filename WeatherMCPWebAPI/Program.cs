using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using WeatherMCPWebAPI.Application.Interfaces;
using WeatherMCPWebAPI.Application.Services;
using WeatherMCPWebAPI.Infrastructure.External;
using WeatherMCPWebAPI.Tools;

// Detect if running as MCP server and configure accordingly
var isMcpMode = args.Length == 0 && Environment.GetEnvironmentVariable("TERM_PROGRAM") == null;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
});

// Suppress startup messages in MCP mode
if (isMcpMode)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(System.Net.IPAddress.Loopback, 0); // Use any available port, we don't need HTTP in MCP mode
    });
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for Claude Desktop
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure HttpClient for weather.gov API
builder.Services.AddHttpClient<IWeatherApiClient, NationalWeatherServiceClient>();

// Register application services
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Register WeatherTools for direct endpoint access
builder.Services.AddScoped<WeatherTools>();

// Configure MCP Server with stdio transport for Claude Desktop
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Add health checks
builder.Services.AddHealthChecks();

// Configure logging - suppress console output for MCP stdio mode
builder.Logging.ClearProviders();
if (!isMcpMode)
{
    builder.Logging.AddConsole(); // Only add console logging when not in MCP mode
}

var app = builder.Build();

// Configure the HTTP request pipeline only when not in MCP mode
if (!isMcpMode)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Add tools debug endpoint
    app.MapGet("/mcp/tools", () =>
    {
        var tools = new[]
        {
            new { name = "GetCurrentWeather", description = "Get current weather conditions for a specified city" },
            new { name = "GetWeatherForecast", description = "Get weather forecast for the next 1-5 days" },
            new { name = "GetWeatherAlerts", description = "Get active weather alerts and warnings" },
            new { name = "CompareWeather", description = "Compare weather between two cities" },
            new { name = "GetWeatherHistory", description = "Get historical weather data" },
            new { name = "ConvertTemperature", description = "Convert temperature between units" },
            new { name = "CalculateHeatIndex", description = "Calculate heat index from temperature and humidity" },
            new { name = "GetSunriseSunset", description = "Get sunrise and sunset times" }
        };
        return Results.Ok(new { tools });
    });

    // Add simple tool testing endpoints
    app.MapGet("/weather/current/{city}", async ([FromServices] WeatherTools weatherTools, string city) =>
    {
        var result = await weatherTools.GetCurrentWeather(city);
        return Results.Ok(new { city, weather = result });
    });

    app.MapGet("/weather/forecast/{city}", async ([FromServices] WeatherTools weatherTools, string city, int days = 5) =>
    {
        var result = await weatherTools.GetWeatherForecast(city, days);
        return Results.Ok(new { city, days, forecast = result });
    });

    app.MapGet("/test", () => "Weather MCP Web API is running!");
}

// MCP Server handles stdio communication automatically regardless of mode
app.Run();