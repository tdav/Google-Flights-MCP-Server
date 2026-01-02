# Google Flights MCP Server (.NET 8.0)

[![CI](https://github.com/tdav/Google-Flights-MCP-Server/actions/workflows/ci.yml/badge.svg)](https://github.com/tdav/Google-Flights-MCP-Server/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A comprehensive Model Context Protocol (MCP) Server for Google Flights integration, built with C# .NET 8.0. This solution includes a Web API with search history tracking, real-time flight scraping, and IP address logging.

## 🚀 Features

- **Real-time Flight Scraping** - Uses Playwright and AngleSharp to fetch live flight data from Google Flights.
- **RESTful Web API** - Full-featured flight search API with Swagger documentation.
- **Search History Tracking** - Persistent storage of all flight searches with client IP addresses.
- **PostgreSQL Database** - Production-ready database with Entity Framework Core.
- **Request Logging** - Comprehensive middleware for tracking all API requests.
- **Console MCP Server** - Command-line interface for MCP protocol integration.
- **Docker Support** - Complete containerization with Docker Compose.
- **CI/CD Pipeline** - GitHub Actions workflow for automated testing.
- **Comprehensive Testing** - Unit and integration tests with xUnit and WebApplicationFactory.

## 📁 Project Structure

```
Google-Flights-MCP-Server/
├── GoogleFlights.Core/             # Shared models, entities, and core business logic
│   ├── Services/
│   │   ├── IFlightSearchService.cs
│   │   └── FlightSearchService.cs  # Main scraping logic using Playwright
│   ├── Models/                     # Core data models
│   └── Helpers/                    # Date and Price helpers
├── GoogleFlightsApi/               # Web API Project (ASP.NET Core)
│   ├── Controllers/                # API endpoints (Flights, History, Health)
│   ├── Data/                       # EF Core DbContext and Entities
│   ├── Services/                   # API-specific services (Tracking, History)
│   └── Middleware/                 # Request logging middleware
├── GoogleFlightsMcp/               # Console MCP Server
│   ├── Mcp/                        # MCP Protocol implementation
│   └── Tools/                      # MCP Tool definitions
├── GoogleFlightsApi.Tests/         # Unit and Integration tests
│   ├── Services/                   # Service unit tests
│   └── Integration/                # API integration tests
├── docker/                         # Dockerfile
├── docker-compose.yml              # Multi-container setup
└── GoogleFlightsMcp.sln            # Solution file
```

## 🛠️ Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
- [PostgreSQL 16](https://www.postgresql.org/download/) (optional - uses in-memory DB if not configured).
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment).
- **Playwright Browsers** (Required for scraping).

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

### 3. Install Playwright Browsers

```bash
# Install Playwright CLI tool if not already installed
dotnet tool install --global Microsoft.Playwright.CLI

# Install Chromium browser
powershell -ExecutionPolicy Bypass -File GoogleFlightsMcp/bin/Debug/net8.0/playwright.ps1 install chromium
```

### 4. Run the Web API

#### Option A: With In-Memory Database (Development)

```bash
cd GoogleFlightsApi
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
cd GoogleFlightsApi
dotnet ef database update  # Apply migrations
dotnet run
```

#### Option C: With Docker Compose

```bash
docker-compose up
```

The API will be available at `http://localhost:8080`

### 5. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:8080/swagger
```


**Request Example (GET):**
```bash
curl "http://localhost:8080/api/flights/search?origin=TAS&destination=JFK&departureDate=2026-01-09&returnDate=2026-02-15&passengers=1&cabinClass=economy"
//https://www.google.ca/travel/flights/search?tfs=CBwQAhojEgoyMDI2LTAxLTA5agwIAhIIL20vMGZzbXlyBwgBEgNKRksaIxIKMjAyNi0wMi0xNWoHCAESA0pGS3IMCAISCC9tLzBmc215QAFIAXABggELCP___________wGYAQE&hl=en&curr=USD
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
  "searchUrl": "https://www.google.ca/travel/flights/search?tfs=CBwQAhojEgoyMDI2LTAxLTA5agwIAhIIL20vMGZzbXlyBwgBEgNKRksaIxIKMjAyNi0wMi0xNWoHCAESA0pGS3IMCAISCC9tLzBmc215QAFIAXABggELCP___________wGYAQE&hl=en&curr=USD"
}
```

 



## 📚 API Endpoints

### Root Endpoint
- **GET /** - Server information and available endpoints

### Flight Search
- **POST /api/flights/search** - Search for flights (JSON body)
- **GET /api/flights/search** - Search for flights (query parameters)

**Request Example (GET):**
```bash
curl "http://localhost:8080/api/flights/search?origin=TAS&destination=JFK&departureDate=2026-01-09&returnDate=2026-02-15&passengers=1&cabinClass=economy"
```

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test GoogleFlightsApi.Tests/GoogleFlightsApi.Tests.csproj
```

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=google_flights;Username=postgres;Password=postgres"
  },
  "GoogleFlights": {
    "ServiceName": "Google Flights MCP Server",
    "Version": "1.0.0",
    "MaxSearchResults": 10
  }
}
```

## 🐳 Docker Deployment

### Build Docker Image

```bash
docker build -f docker/Dockerfile -t google-flights-api:latest .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

## 🧩 NuGet Packages Used

### Core & Web API
- Microsoft.Playwright (1.57.0)
- AngleSharp (1.4.0)
- Microsoft.EntityFrameworkCore (8.0.11)
- Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11)
- Serilog.AspNetCore (8.0.3)

### Testing
- Microsoft.AspNetCore.Mvc.Testing (8.0.11)
- FluentAssertions (6.12.2)
- Moq (4.20.72)
- Microsoft.EntityFrameworkCore.InMemory (8.0.11)

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Logic ported and inspired by the [Google Flights MCP Server](https://github.com/opspawn/Google-Flights-MCP-Server) Python implementation.
- Built with .NET 8.0 and ASP.NET Core.
- Uses Model Context Protocol (MCP) for AI integration.

---

Made with ❤️ using C# and .NET 8.0