using System.Web;

namespace GoogleFlightsMCP.Server.Services;

public class FlightSearchService : IFlightSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlightSearchService> _logger;

    public FlightSearchService(HttpClient httpClient, ILogger<FlightSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FlightSearchResult> SearchFlightsAsync(
        string origin,
        string destination,
        string departureDate,
        string? returnDate,
        int passengers,
        string cabinClass)
    {
        // Validate inputs
        ValidateInputs(origin, destination, departureDate, passengers, cabinClass);

        // Build Google Flights URL
        var searchUrl = BuildGoogleFlightsUrl(origin, destination, departureDate, returnDate, passengers, cabinClass);

        _logger.LogInformation("Searching flights: {Origin} -> {Destination} on {DepartureDate}", 
            origin, destination, departureDate);

        // Note: This is a demonstration implementation
        // In a real-world scenario, you would need to:
        // 1. Use Google's official Flight Search API (requires API key)
        // 2. Or use a web scraping solution (subject to Google's Terms of Service)
        // 3. Or integrate with a flight data provider (e.g., Skyscanner, Amadeus, etc.)

        var flights = await SimulateFlightSearch(origin, destination, departureDate, returnDate);

        return new FlightSearchResult(
            origin,
            destination,
            departureDate,
            returnDate,
            passengers,
            cabinClass,
            flights,
            searchUrl
        );
    }

    private void ValidateInputs(string origin, string destination, string departureDate, int passengers, string cabinClass)
    {
        if (string.IsNullOrWhiteSpace(origin))
            throw new ArgumentException("Origin airport code is required", nameof(origin));

        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination airport code is required", nameof(destination));

        if (string.IsNullOrWhiteSpace(departureDate))
            throw new ArgumentException("Departure date is required", nameof(departureDate));

        if (!DateTime.TryParse(departureDate, out _))
            throw new ArgumentException("Invalid departure date format", nameof(departureDate));

        if (passengers < 1 || passengers > 9)
            throw new ArgumentException("Passengers must be between 1 and 9", nameof(passengers));

        var validCabinClasses = new[] { "economy", "premium_economy", "business", "first" };
        if (!validCabinClasses.Contains(cabinClass.ToLower()))
            throw new ArgumentException($"Cabin class must be one of: {string.Join(", ", validCabinClasses)}", nameof(cabinClass));
    }

    private string BuildGoogleFlightsUrl(
        string origin,
        string destination,
        string departureDate,
        string? returnDate,
        int passengers,
        string cabinClass)
    {
        var baseUrl = "https://www.google.com/travel/flights";
        
        // Format: /flights?q=Flights%20from%20JFK%20to%20LAX%20on%202024-12-15
        var query = $"Flights from {origin} to {destination} on {departureDate}";
        
        if (!string.IsNullOrWhiteSpace(returnDate))
        {
            query += $" returning {returnDate}";
        }

        var encodedQuery = HttpUtility.UrlEncode(query);
        var url = $"{baseUrl}?q={encodedQuery}&hl=en&curr=USD&passengers={passengers}&cabin={cabinClass}";

        return url;
    }

    private Task<List<Flight>> SimulateFlightSearch(
        string origin,
        string destination,
        string departureDate,
        string? returnDate)
    {
        // This is a mock implementation
        // In a real implementation, you would fetch actual flight data
        var random = new Random();
        var airlines = new[] { "United Airlines", "Delta", "American Airlines", "Southwest", "JetBlue" };
        var flights = new List<Flight>();

        for (int i = 0; i < 5; i++)
        {
            var airline = airlines[random.Next(airlines.Length)];
            var departureHour = random.Next(6, 22);
            var duration = random.Next(2, 8);
            var arrivalHour = (departureHour + duration) % 24;
            var stops = random.Next(0, 3);
            var basePrice = 200 + random.Next(100, 800);

            flights.Add(new Flight(
                Airline: airline,
                FlightNumber: $"{airline.Substring(0, Math.Min(airline.Length, 2)).ToUpper()}{random.Next(1000, 9999)}",
                DepartureTime: $"{departureHour:D2}:{random.Next(0, 60):D2}",
                ArrivalTime: $"{arrivalHour:D2}:{random.Next(0, 60):D2}",
                Duration: $"{duration}h {random.Next(0, 60)}m",
                Stops: stops,
                Price: basePrice,
                Currency: "USD"
            ));
        }

        return Task.FromResult(flights.OrderBy(f => f.Price).ToList());
    }
}
