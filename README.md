# Google-Flights-MCP-Server

A Model Context Protocol (MCP) Server for Google Flights integration, built with C# .NET 8.0.

## Project Structure

The project is located in the `GoogleFlightsMCP.Server` directory. This repository contains:

```
Google-Flights-MCP-Server/
├── GoogleFlightsMCP.sln                    # Solution file
├── GoogleFlightsMCP.Server/                # Main MCP Server project
│   ├── Program.cs                          # Application entry point with API endpoints
│   ├── Services/                           # Business logic services
│   │   ├── IFlightSearchService.cs        # Flight search interface
│   │   └── FlightSearchService.cs         # Flight search implementation
│   ├── appsettings.json                   # Application configuration
│   └── GoogleFlightsMCP.Server.csproj     # Project file
└── README.md                               # This file
```

## Features

- **MCP Server**: Implements the Model Context Protocol for flight searches
- **Google Flights Integration**: Generates Google Flights search URLs
- **RESTful API**: Provides HTTP endpoints for flight searches
- **Swagger UI**: Interactive API documentation
- **CORS Support**: Allows cross-origin requests
- **Configurable**: Support for different cabin classes, passenger counts, and trip types

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/tdav/Google-Flights-MCP-Server.git
cd Google-Flights-MCP-Server
```

### 2. Build the Project

```bash
dotnet build
```

### 3. Run the Server

```bash
cd GoogleFlightsMCP.Server
dotnet run
```

The server will start on `http://localhost:5000` (or `https://localhost:5001` for HTTPS).

### 4. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

## API Endpoints

### GET /
Returns server information (name, version, description).

### GET /api/flights/search
Search for flights with query parameters:
- `origin` (required): Origin airport code (e.g., "JFK")
- `destination` (required): Destination airport code (e.g., "LAX")
- `departureDate` (required): Departure date (YYYY-MM-DD)
- `returnDate` (optional): Return date for round trips
- `passengers` (required): Number of passengers (1-9)
- `cabinClass` (required): Cabin class (economy, premium_economy, business, first)

**Example:**
```bash
curl "http://localhost:5000/api/flights/search?origin=JFK&destination=LAX&departureDate=2024-12-15&passengers=1&cabinClass=economy"
```

### POST /api/mcp/search
MCP-compatible endpoint for flight searches. Accepts JSON body:

```json
{
  "origin": "JFK",
  "destination": "LAX",
  "departureDate": "2024-12-15",
  "returnDate": "2024-12-20",
  "passengers": 2,
  "cabinClass": "economy"
}
```

**Example:**
```bash
curl -X POST http://localhost:5000/api/mcp/search \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "JFK",
    "destination": "LAX",
    "departureDate": "2024-12-15",
    "passengers": 1,
    "cabinClass": "economy"
  }'
```

## Response Format

```json
{
  "origin": "JFK",
  "destination": "LAX",
  "departureDate": "2024-12-15",
  "returnDate": null,
  "passengers": 1,
  "cabinClass": "economy",
  "flights": [
    {
      "airline": "United Airlines",
      "flightNumber": "UA1234",
      "departureTime": "08:30",
      "arrivalTime": "11:45",
      "duration": "5h 15m",
      "stops": 0,
      "price": 250.00,
      "currency": "USD"
    }
  ],
  "searchUrl": "https://www.google.com/travel/flights?q=..."
}
```

## Configuration

Edit `appsettings.json` to configure the server:

```json
{
  "GoogleFlightsMCP": {
    "ServerName": "Google Flights MCP Server",
    "Version": "1.0.0",
    "Description": "Model Context Protocol Server for Google Flights integration"
  }
}
```

## Development

### Run in Development Mode

```bash
dotnet run --environment Development
```

### Watch Mode (auto-reload on changes)

```bash
dotnet watch run
```

### Build for Release

```bash
dotnet build --configuration Release
```

## Implementation Notes

This is a demonstration implementation. The current version:
- ✅ Provides a fully functional MCP server structure
- ✅ Validates input parameters
- ✅ Generates valid Google Flights search URLs
- ✅ Returns simulated flight data
- ⚠️ Uses mock data for flight results (for demonstration purposes)

For production use, you would need to integrate with:
- Google's official Flight Search API
- Third-party flight data providers (Skyscanner, Amadeus, etc.)
- Web scraping solutions (subject to Google's Terms of Service)

## License

This project is open source and available under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions, please open an issue on GitHub.
