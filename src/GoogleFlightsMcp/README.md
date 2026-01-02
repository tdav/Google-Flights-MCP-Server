# GoogleFlightsMcp - Console MCP Server

A Model Context Protocol (MCP) server for Google Flights integration, built with C# .NET 8.0. This console application implements the MCP protocol for flight search operations.

## Features

- **MCP Protocol Support** - Full JSON-RPC 2.0 implementation over STDIO
- **Flight Search** - Search for flights between airports
- **URL Generation** - Generate Google Flights URLs
- **Trip Types** - Support for one-way and round-trip flights
- **Cabin Classes** - Economy, Premium Economy, Business, and First class
- **Passenger Management** - Support for adults, children, and infants

## Architecture

### Models
- **Flight** - Individual flight details
- **FlightData** - Flight search parameters
- **FlightResult** - Search results wrapper
- **Passengers** - Passenger counts (adults, children, infants)
- **SeatType** - Enum for cabin classes
- **TripType** - Enum for trip types

### Services
- **FlightSearchService** - Core flight search logic
- Airport code to Google code mapping
- URL generation for Google Flights
- Validation of search parameters

### Helpers
- **DateHelper** - Date formatting and validation utilities
- **PriceHelper** - Price formatting and currency conversion

### MCP Components
- **McpServer** - MCP protocol server implementation
- **McpMessage** - Protocol message models
- JSON-RPC 2.0 request/response handling
- Tool execution engine

### Tools
- **FlightTools** - MCP tool definitions
  - `search_flights` - Search for flights
  - `get_flight_url` - Generate Google Flights URL

## Usage

### Running the Server

```bash
cd src/GoogleFlightsMcp
dotnet run
```

The server listens on STDIN and writes responses to STDOUT, with logs going to STDERR.

### MCP Protocol Communication

#### Initialize
```json
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}
```

Response:
```json
{
  "jsonrpc":"2.0",
  "id":1,
  "result":{
    "name":"Google Flights MCP Server",
    "version":"1.0.0",
    "protocolVersion":"2024-11-05",
    "capabilities":{"tools":{"listChanged":false}}
  }
}
```

#### List Tools
```json
{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
```

#### Search Flights (Test Data: TAS ↔ JFK)
```json
{
  "jsonrpc":"2.0",
  "id":3,
  "method":"tools/call",
  "params":{
    "name":"search_flights",
    "arguments":{
      "origin":"TAS",
      "destination":"JFK",
      "departureDate":"2026-01-09",
      "returnDate":"2026-02-15",
      "passengers":1,
      "cabinClass":"economy",
      "tripType":"round_trip"
    }
  }
}
```

#### Get Flight URL
```json
{
  "jsonrpc":"2.0",
  "id":4,
  "method":"tools/call",
  "params":{
    "name":"get_flight_url",
    "arguments":{
      "origin":"TAS",
      "destination":"JFK",
      "departureDate":"2026-01-09",
      "returnDate":"2026-02-15",
      "passengers":1,
      "cabinClass":"economy"
    }
  }
}
```

## Test Data

### Example: Tashkent ↔ New York
- **Route:** TAS (Tashkent) ↔ JFK (New York)
- **Dates:** 2026-01-09 → 2026-02-15
- **Airport Codes:**
  - TAS: `/m/0fsmy`
  - JFK: `/m/02_286`
- **Expected Price Range:** ~$1,150 - $1,500 USD (economy)

### Testing with Test Script

```bash
# Initialize server
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | dotnet run

# Search flights
echo '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"search_flights","arguments":{"origin":"TAS","destination":"JFK","departureDate":"2026-01-09","returnDate":"2026-02-15","passengers":1,"cabinClass":"economy","tripType":"round_trip"}}}' | dotnet run
```

## Supported Airport Codes

The service includes a comprehensive list of airport codes and their Google codes:

### North America
- JFK, LAX, ORD, DFW, SFO, SEA, MIA, BOS, ATL, LAS, PHX, DEN, etc.

### Europe
- LHR, CDG, FRA, AMS, MAD, BCN, FCO, MUC, IST, etc.

### Asia
- DXB, HKG, NRT, SIN, ICN, BKK, DEL, PEK, PVG, HND

### Central Asia
- **TAS** (Tashkent) - `/m/0fsmy`

### Other Regions
- Australia, South America, Africa

## Configuration

The server uses Serilog for logging, configured to write to STDERR to avoid interfering with the MCP protocol on STDOUT.

### Log Levels
- Information - General server operations
- Warning - Invalid requests or parsing errors
- Error - Tool execution errors
- Fatal - Critical server errors

## Dependencies

- .NET 8.0 SDK
- Serilog 4.1.0
- Serilog.Sinks.Console 6.0.0
- AngleSharp 1.1.2 (for future HTML parsing)

## Integration with MCP Clients

To use this server with MCP clients (like Claude Desktop or Cline), add it to your client configuration:

### Claude Desktop Configuration

```json
{
  "mcpServers": {
    "google-flights": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/GoogleFlightsMcp/GoogleFlightsMcp.csproj"],
      "env": {}
    }
  }
}
```

### Cline Configuration

```json
{
  "mcpServers": [
    {
      "name": "google-flights",
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/GoogleFlightsMcp/GoogleFlightsMcp.csproj"]
    }
  ]
}
```

## Development

### Building
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Project Structure
```
GoogleFlightsMcp/
├── Models/           # Data models
├── Services/         # Business logic
├── Helpers/          # Utility functions
├── Mcp/             # MCP protocol implementation
├── Tools/           # MCP tool definitions
└── Program.cs       # Entry point
```

## API Response Format

All tool responses follow the MCP content format:

```json
{
  "jsonrpc":"2.0",
  "id":3,
  "result":{
    "content":[
      {
        "type":"text",
        "text":"# Flight Search Results\n\n**Route:** TAS → JFK\n..."
      }
    ],
    "isError":false
  }
}
```

## License

MIT License - See LICENSE file for details.

## References

- [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
- [Google Flights](https://www.google.com/travel/flights)
- [Original Python Implementation](https://github.com/opspawn/Google-Flights-MCP-Server)

---

Built with ❤️ using C# and .NET 8.0
