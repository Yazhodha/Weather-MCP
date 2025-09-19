# Weather MCP Web API Server

A comprehensive Model Context Protocol (MCP) server for weather data, built with .NET 8 and Clean Architecture principles. This server provides Claude Desktop with access to real-time weather information, forecasts, alerts, and weather-related calculations.

## 🌤️ Features

### Weather Tools
- **Current Weather** - Get real-time weather conditions for any city
- **Weather Forecast** - 1-5 day weather forecasts with detailed information
- **Weather Alerts** - Active weather warnings and alerts for any region
- **Weather Comparison** - Side-by-side weather comparison between cities
- **Weather History** - Historical weather data (simulated for demonstration)
- **Temperature Conversion** - Convert between Celsius, Fahrenheit, and Kelvin
- **Heat Index Calculator** - Calculate apparent temperature from temperature and humidity
- **Sunrise/Sunset Times** - Get sunrise and sunset times for any city (simulated)

### Technical Features
- **Clean Architecture** - Proper separation of concerns with Domain, Application, Infrastructure, and API layers
- **Real Weather Data** - Integration with National Weather Service (weather.gov) API
- **Fallback Simulation** - Graceful degradation with simulated data when external APIs are unavailable
- **MCP Protocol Compliance** - Full support for Model Context Protocol with Claude Desktop
- **Comprehensive Error Handling** - Robust error handling with informative responses
- **Dependency Injection** - Modern .NET DI container with proper service registration

## 🏗️ Architecture

This project follows Clean Architecture principles:

```
├── WeatherMCPWebAPI.Domain/          # Core business logic and entities
│   ├── Entities/                     # Weather, WeatherForecast, WeatherAlert
│   └── ValueObjects/                 # Temperature, Location
├── WeatherMCPWebAPI.Application/     # Application services and interfaces
│   ├── Interfaces/                   # IWeatherService, IWeatherApiClient
│   ├── Services/                     # WeatherService business logic
│   └── Models/                       # DTOs and API models
├── WeatherMCPWebAPI.Infrastructure/  # External integrations
│   ├── External/                     # NationalWeatherServiceClient
│   └── Services/                     # Infrastructure services
└── WeatherMCPWebAPI/                 # API layer and MCP tools
    ├── Tools/                        # MCP tool implementations
    └── Program.cs                    # Application startup
```

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Claude Desktop](https://claude.ai/download) (for MCP integration)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Yazhodha/weather-mcp-webapi.git
   cd weather-mcp-webapi
   ```

2. **Build the project**
   ```bash
   dotnet build --configuration Release
   ```

3. **Publish for MCP usage**
   ```bash
   cd WeatherMCPWebAPI
   dotnet publish --configuration Release --self-contained false --output ../published
   ```

### Claude Desktop Configuration

Add this configuration to your `claude_desktop_config.json`:

**Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
**macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`
**Linux:** `~/.config/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "weather-mcp-server": {
      "command": "dotnet",
      "args": ["path/to/published/WeatherMCPWebAPI.dll"],
      "cwd": "path/to/published"
    }
  }
}
```

Replace `path/to/published` with the actual path to your published directory.

### Usage

1. **Restart Claude Desktop** after updating the configuration
2. **Ask weather questions** in Claude Desktop:
   - "What's the weather in New York?"
   - "Give me a 5-day forecast for London"
   - "Compare weather between Seattle and Miami"
   - "Convert 75 degrees Fahrenheit to Celsius"
   - "Calculate heat index for 90°F and 70% humidity"

## 🛠️ Development

### Running in Development Mode

For local development with full HTTP API access:

```bash
cd WeatherMCPWebAPI
dotnet run
```

This provides:
- Swagger UI at `https://localhost:7042/swagger`
- Health check at `https://localhost:7042/health`
- Direct weather endpoints:
  - `GET /weather/current/{city}`
  - `GET /weather/forecast/{city}?days=3`
- Tools debug endpoint at `https://localhost:7042/mcp/tools`

### Project Structure

```
WeatherMCPWebAPI/
├── WeatherMCPWebAPI.Domain/
│   ├── Entities/
│   │   ├── Weather.cs
│   │   ├── WeatherForecast.cs
│   │   └── WeatherAlert.cs
│   └── ValueObjects/
│       ├── Temperature.cs
│       └── Location.cs
├── WeatherMCPWebAPI.Application/
│   ├── Interfaces/
│   │   ├── IWeatherService.cs
│   │   └── IWeatherApiClient.cs
│   ├── Services/
│   │   └── WeatherService.cs
│   └── Models/
│       └── WeatherApiModels.cs
├── WeatherMCPWebAPI.Infrastructure/
│   └── External/
│       └── NationalWeatherServiceClient.cs
└── WeatherMCPWebAPI/
    ├── Tools/
    │   └── WeatherTools.cs
    └── Program.cs
```

### Key Dependencies

- **ModelContextProtocol** (preview) - MCP server implementation
- **Microsoft.Extensions.Hosting** - Hosting and DI
- **Microsoft.Extensions.Http** - HTTP client factory

## 🌍 Weather Data Sources

### Primary Source: National Weather Service
- **API**: https://api.weather.gov
- **Coverage**: United States
- **Features**: Real-time weather, forecasts, alerts
- **Rate Limiting**: Respectful API usage with proper headers

### Fallback Data
When the National Weather Service API is unavailable:
- **Simulated Weather Data** - Realistic weather patterns
- **City Coordinates** - Built-in lookup for major US cities
- **Graceful Degradation** - No service interruption

## 📖 API Documentation

### MCP Tools

| Tool Name | Description | Parameters |
|-----------|-------------|------------|
| `get_current_weather` | Current weather conditions | `city: string` |
| `get_weather_forecast` | Weather forecast | `city: string`, `days: number (1-5)` |
| `get_weather_alerts` | Active weather alerts | `region: string` |
| `compare_weather` | Compare two cities | `city1: string`, `city2: string` |
| `get_weather_history` | Historical weather data | `city: string`, `startDate: string`, `endDate: string` |
| `convert_temperature` | Temperature unit conversion | `value: number`, `fromUnit: string`, `toUnit: string` |
| `calculate_heat_index` | Heat index calculation | `temperatureF: number`, `humidity: number` |
| `get_sunrise_sunset` | Sunrise/sunset times | `city: string` |

### Example Responses

**Current Weather**
```
🌤️  Current Weather for London, UK
Temperature: 22°C (72°F)
Conditions: Partly Cloudy
Humidity: 65%
Pressure: 1013 hPa
Wind: 15 km/h NW
Last Updated: 2024-01-15 14:30:00 UTC
```

**Weather Comparison**
```
🔍 Weather Comparison
═══════════════════════════════════════

📍 New York, US vs London, UK

🌡️  Temperature:
   • New York: 25.0°C (77.0°F)
   • London: 18.0°C (64.4°F)
   • Difference: 7.0°C

💧 Humidity:
   • New York: 68%
   • London: 72%
```

## 🔧 Configuration

### Environment Variables

- **MCP_MODE**: Set to disable console logging for MCP usage
- **ASPNETCORE_ENVIRONMENT**: Set to `Development` for full HTTP API

### Logging

The application automatically detects MCP mode and suppresses console logging to ensure clean JSON-RPC communication with Claude Desktop.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **National Weather Service** for providing free weather data API
- **Anthropic** for the Claude Desktop and MCP protocol
- **Microsoft** for the excellent .NET ecosystem and MCP SDK

## 🐛 Troubleshooting

### Common Issues

**Claude Desktop shows "Server disconnected"**
- Ensure the path in `claude_desktop_config.json` is correct
- Check that .NET 8 runtime is installed
- Verify the published DLL exists at the specified path

**JSON parsing errors in Claude Desktop logs**
- Make sure you're using the published DLL, not `dotnet run`
- Restart Claude Desktop after configuration changes

**Weather data unavailable**
- The server gracefully falls back to simulated data
- Check internet connectivity for real weather.gov data
- Rate limiting may cause temporary fallback to simulated data

### Getting Help

- Check the [Issues](https://github.com/Yazhodha/weather-mcp-webapi/issues) page
- Review Claude Desktop [MCP documentation](https://modelcontextprotocol.io/)
- Ensure your .NET 8 installation is up to date

---

**Built with ❤️ using .NET 8 and Clean Architecture**
