# Google Flights MCP Server (.NET 8.0)

[![CI](https://github.com/tdav/Google-Flights-MCP-Server/actions/workflows/ci.yml/badge.svg)](https://github.com/tdav/Google-Flights-MCP-Server/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comprehensive Model Context Protocol (MCP) Server for Google Flights integration, built with C# .NET 8.0. This solution includes a Web API with search history tracking and IP address logging.

## 🚀 Features

- **RESTful Web API** - Full-featured flight search API with Swagger documentation
- **Search History Tracking** - Persistent storage of all flight searches with client IP addresses
- **PostgreSQL Database** - Production-ready database with Entity Framework Core
- **Request Logging** - Comprehensive middleware for tracking all API requests
- **Console MCP Server** - Command-line interface for MCP protocol integration
- **Docker Support** - Complete containerization with Docker Compose
- **CI/CD Pipeline** - GitHub Actions workflow for automated testing
- **Comprehensive Testing** - Unit and integration tests with xUnit

## 📁 Project Structure

```
Google-Flights-MCP-Server/
├── src/
│   ├── GoogleFlightsMcp/           # Console MCP Server
│   │   ├── Program.cs
│   │   └── GoogleFlightsMcp.csproj
│   └── GoogleFlightsApi/           # Web API
│       ├── Program.cs
│       ├── appsettings.json
│       ├── Controllers/
│       │   ├── FlightsController.cs
│       │   ├── HistoryController.cs
│       │   └── HealthController.cs
│       ├── Models/
│       │   ├── FlightSearchRequest.cs
│       │   ├── FlightSearchResponse.cs
│       │   ├── AirportCodes.cs
│       │   └── HistoryDtos.cs
│       ├── Services/
│       │   ├── IFlightSearchService.cs
│       │   ├── FlightSearchService.cs
│       │   ├── IClientTrackingService.cs
│       │   ├── ClientTrackingService.cs
│       │   ├── ISearchHistoryService.cs
│       │   └── SearchHistoryService.cs
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Entities/
│       │       ├── ClientInfo.cs
│       │       ├── SearchHistory.cs
│       │       ├── FlightResult.cs
│       │       └── RequestLog.cs
│       ├── Middleware/
│       │   └── RequestLoggingMiddleware.cs
│       └── Logging/
│           └── SerilogConfiguration.cs
├── tests/
│   └── GoogleFlightsApi.Tests/
│       ├── Services/
│       │   ├── FlightSearchServiceTests.cs
│       │   └── ClientTrackingServiceTests.cs
│       └── GoogleFlightsApi.Tests.csproj
├── docker/
│   └── Dockerfile
├── docker-compose.yml
├── .github/
│   └── workflows/
│       └── ci.yml
└── GoogleFlightsMcp.sln
```

## 🛠️ Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [PostgreSQL 16](https://www.postgresql.org/download/) (optional - uses in-memory DB if not configured)
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/tdav/Google-Flights-MCP-Server.git
cd Google-Flights-MCP-Server
```

### 2. Build the Solution

```bash
dotnet restore
dotnet build
```

### 3. Run the Web API

#### Option A: With In-Memory Database (Development)

```bash
cd src/GoogleFlightsApi
dotnet run
```

#### Option B: With PostgreSQL (Production)

First, update `appsettings.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=google_flights;Username=postgres;Password=yourpassword"
  }
}
```

Then run:

```bash
cd src/GoogleFlightsApi
dotnet ef database update  # Apply migrations
dotnet run
```

#### Option C: With Docker Compose

```bash
docker-compose up
```

The API will be available at `http://localhost:8080`

### 4. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:8080/swagger
```

## 📚 API Endpoints

### Root Endpoint
- **GET /** - Server information and available endpoints

### Health Check
- **GET /api/health** - Health check endpoint
- **GET /api/health/ping** - Simple ping/pong endpoint

### Flight Search
- **POST /api/flights/search** - Search for flights (JSON body)
- **GET /api/flights/search** - Search for flights (query parameters)

**Request Example (POST):**
```json
{
  "origin": "TAS",
  "destination": "JFK",
  "departureDate": "2026-01-09",
  "returnDate": "2026-02-15",
  "passengers": 1,
  "cabinClass": "economy"
}
```

**Request Example (GET):**
```bash
curl "http://localhost:8080/api/flights/search?origin=TAS&destination=JFK&departureDate=2026-01-09&returnDate=2026-02-15&passengers=1&cabinClass=economy"
```

**Response Example:**
```json
{
  "origin": "TAS",
  "destination": "JFK",
  "departureDate": "2026-01-09",
  "returnDate": "2026-02-15",
  "passengers": 1,
  "cabinClass": "economy",
  "flights": [
    {
      "airline": "Emirates",
      "flightNumber": "EK5421",
      "departureTime": "14:30",
      "arrivalTime": "22:15",
      "duration": "13h 45m",
      "stops": 1,
      "price": 1250.00,
      "currency": "USD"
    }
  ],
  "searchUrl": "https://www.google.ca/travel/flights/search?tfs=..."
}
```

### Search History
- **GET /api/history/my** - Get search history for current IP address
- **GET /api/history/all** - Get all search history (admin endpoint)

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10 for /my, 50 for /all)

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/GoogleFlightsApi.Tests/GoogleFlightsApi.Tests.csproj
```

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=google_flights;Username=postgres;Password=postgres"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://yourdomain.com"
    ]
  },
  "GoogleFlights": {
    "ServiceName": "Google Flights MCP Server",
    "Version": "1.0.0",
    "MaxSearchResults": 10
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Application environment (Development/Production)
- `ConnectionStrings__DefaultConnection` - Database connection string
- `ASPNETCORE_URLS` - URLs to listen on (default: http://+:8080)

## 🗄️ Database Schema

### ClientInfo
- Tracks unique clients by IP address
- Stores user agent and access timestamps
- Maintains search count per client

### SearchHistory
- Stores all flight search queries
- Links to ClientInfo
- Includes search parameters and Google Flights URL

### FlightResult
- Stores individual flight results
- Links to SearchHistory
- Contains flight details (airline, price, etc.)

### RequestLog
- Logs all API requests
- Includes method, path, status code, duration
- Optional link to ClientInfo

## 🐳 Docker Deployment

### Build Docker Image

```bash
docker build -f docker/Dockerfile -t google-flights-api:latest .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

Services:
- **API**: `http://localhost:8080`
- **PostgreSQL**: `localhost:5432`

### Stop Services

```bash
docker-compose down
```

## 📊 Database Migrations

### Create a new migration

```bash
cd src/GoogleFlightsApi
dotnet ef migrations add InitialCreate
```

### Apply migrations

```bash
dotnet ef database update
```

### Remove last migration

```bash
dotnet ef migrations remove
```

## 🧩 NuGet Packages Used

### Web API
- Microsoft.EntityFrameworkCore (8.0.11)
- Microsoft.EntityFrameworkCore.Design (8.0.11)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- Serilog (4.1.0)
- Serilog.AspNetCore (8.0.3)
- Serilog.Sinks.Console (6.0.0)
- Serilog.Sinks.File (6.0.0)
- AngleSharp (1.1.2)
- Swashbuckle.AspNetCore (6.9.0)

### Testing
- Microsoft.NET.Test.Sdk (17.11.1)
- xunit (2.9.2)
- xunit.runner.visualstudio (2.8.2)
- FluentAssertions (6.12.2)
- Moq (4.20.72)
- Microsoft.EntityFrameworkCore.InMemory (8.0.11)

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 Test Data

### Example: Tashkent ↔ New York

```bash
curl -X POST http://localhost:8080/api/flights/search \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "TAS",
    "destination": "JFK",
    "departureDate": "2026-01-09",
    "returnDate": "2026-02-15",
    "passengers": 1,
    "cabinClass": "economy"
  }'
```

**Airport Codes:**
- TAS (Tashkent): `/m/0fsmy`
- JFK (New York): `/m/02_286`

**Google Flights URL:**
```
https://www.google.ca/travel/flights/search?tfs=CBwQAhopEgoyMDI2LTAxLTA5agwIAxIIL20vMGZzbXlyDQgDEgkvbS8wMl8yODYaKRIKMjAyNi0wMi0xNWoNCAMSCS9tLzAyXzI4NnIMCAMSCC9tLzBmc215QAFIAXABggELCP___________wGYAQE&tfu=EgIIAQ
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Based on the original [Google Flights MCP Server](https://github.com/opspawn/Google-Flights-MCP-Server)
- Built with .NET 8.0 and ASP.NET Core
- Uses Model Context Protocol (MCP) for AI integration

## 📞 Support

For issues and questions, please open an issue on GitHub.

## 🔄 Version History

### v1.0.0 (Current)
- Initial C# .NET 8.0 implementation
- Web API with RESTful endpoints
- PostgreSQL database integration
- Search history tracking with IP addresses
- Docker support
- CI/CD pipeline
- Comprehensive testing

---

Made with ❤️ using C# and .NET 8.0
