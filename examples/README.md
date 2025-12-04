# Examples

This directory contains example scripts demonstrating how to use the Google Flights MCP Server API.

## Prerequisites

### For Bash Example
- `curl` - Command-line tool for making HTTP requests
- `jq` - Command-line JSON processor

Install on Ubuntu/Debian:
```bash
sudo apt-get install curl jq
```

Install on macOS:
```bash
brew install curl jq
```

### For Python Example
- Python 3.7 or higher
- `requests` library

Install the required package:
```bash
pip install requests
```

## Running the Examples

First, make sure the server is running:
```bash
cd ../GoogleFlightsMCP.Server
dotnet run
```

The server should start on `http://localhost:5200` (or the port specified in launchSettings.json).

### Bash Example

Run the bash script to test all endpoints:
```bash
./test-api.sh
```

This script will:
1. Get server information
2. Search for one-way flights
3. Search for round-trip flights
4. Test the MCP endpoint
5. Test error handling

### Python Example

Run the Python client:
```bash
python3 example_client.py
```

Or make it executable and run directly:
```bash
chmod +x example_client.py
./example_client.py
```

This script demonstrates:
1. Getting server information
2. Searching for one-way flights with date calculations
3. Searching for round-trip flights
4. Using the MCP endpoint
5. Pretty-printing results

## Example Output

### One-Way Flight Search
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

## Customizing the Examples

You can modify the example scripts to test different:
- **Airport codes**: Change `JFK`, `LAX`, `SFO`, etc.
- **Dates**: Use different departure and return dates
- **Passenger counts**: Test with 1-9 passengers
- **Cabin classes**: 
  - `economy`
  - `premium_economy`
  - `business`
  - `first`

## Integration Examples

### Using with cURL
```bash
# One-way flight
curl "http://localhost:5200/api/flights/search?origin=JFK&destination=LAX&departureDate=2024-12-15&passengers=1&cabinClass=economy"

# Round-trip flight
curl "http://localhost:5200/api/flights/search?origin=JFK&destination=LAX&departureDate=2024-12-15&returnDate=2024-12-22&passengers=2&cabinClass=business"
```

### Using with Python
```python
import requests

response = requests.get("http://localhost:5200/api/flights/search", params={
    "origin": "JFK",
    "destination": "LAX",
    "departureDate": "2024-12-15",
    "passengers": 1,
    "cabinClass": "economy"
})

data = response.json()
print(f"Found {len(data['flights'])} flights")
```

### Using with JavaScript/Node.js
```javascript
const response = await fetch(
  'http://localhost:5200/api/flights/search?' + 
  new URLSearchParams({
    origin: 'JFK',
    destination: 'LAX',
    departureDate: '2024-12-15',
    passengers: '1',
    cabinClass: 'economy'
  })
);

const data = await response.json();
console.log(`Found ${data.flights.length} flights`);
```

## Troubleshooting

### Server Not Running
```
Error: Could not connect to the server
```
**Solution**: Make sure the server is running. Navigate to the `GoogleFlightsMCP.Server` directory and run `dotnet run`.

### Port Already in Use
If port 5200 is already in use, the server will use a different port. Check the server console output for the actual port number and update the `API_BASE_URL` in the example scripts.

### Missing Dependencies
- **Bash**: Install `curl` and `jq`
- **Python**: Install the `requests` library with `pip install requests`

## Contributing

Feel free to add more examples or improve existing ones! See the main [CONTRIBUTING.md](../CONTRIBUTING.md) file for contribution guidelines.
