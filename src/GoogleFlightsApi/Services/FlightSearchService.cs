using GoogleFlightsApi.Models;
using System.Web;

namespace GoogleFlightsApi.Services;

public class FlightSearchService : IFlightSearchService
{
    private readonly ILogger<FlightSearchService> _logger;
    private readonly HttpClient _httpClient;

    public FlightSearchService(
        ILogger<FlightSearchService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request)
    {
        ValidateRequest(request);

        var searchUrl = BuildGoogleFlightsUrl(request);
        _logger.LogInformation(
            "Searching flights: {Origin} -> {Destination} on {DepartureDate}",
            request.Origin, request.Destination, request.DepartureDate);

        // Simulate flight search - in production, integrate with real flight API
        var flights = await SimulateFlightSearch(request);

        return new FlightSearchResponse
        {
            Origin = request.Origin,
            Destination = request.Destination,
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            Passengers = request.Passengers,
            CabinClass = request.CabinClass,
            Flights = flights,
            SearchUrl = searchUrl
        };
    }

    private void ValidateRequest(FlightSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Origin))
            throw new ArgumentException("Origin is required", nameof(request.Origin));

        if (string.IsNullOrWhiteSpace(request.Destination))
            throw new ArgumentException("Destination is required", nameof(request.Destination));

        if (!DateTime.TryParse(request.DepartureDate, out var departureDate))
            throw new ArgumentException("Invalid departure date format", nameof(request.DepartureDate));

        if (departureDate < DateTime.Today)
            throw new ArgumentException("Departure date cannot be in the past", nameof(request.DepartureDate));

        if (!string.IsNullOrWhiteSpace(request.ReturnDate))
        {
            if (!DateTime.TryParse(request.ReturnDate, out var returnDate))
                throw new ArgumentException("Invalid return date format", nameof(request.ReturnDate));

            if (returnDate < departureDate)
                throw new ArgumentException("Return date must be after departure date", nameof(request.ReturnDate));
        }

        var validCabinClasses = new[] { "economy", "premium_economy", "business", "first" };
        if (!validCabinClasses.Contains(request.CabinClass.ToLower()))
            throw new ArgumentException(
                $"Cabin class must be one of: {string.Join(", ", validCabinClasses)}",
                nameof(request.CabinClass));
    }

    private string BuildGoogleFlightsUrl(FlightSearchRequest request)
    {
        // Get Google codes for airports
        var originCode = AirportCodes.GetGoogleCode(request.Origin) ?? $"/m/{request.Origin.ToLower()}";
        var destCode = AirportCodes.GetGoogleCode(request.Destination) ?? $"/m/{request.Destination.ToLower()}";

        // Format: https://www.google.ca/travel/flights/search?tfs=...
        var baseUrl = "https://www.google.ca/travel/flights";
        
        // Build TFS parameter
        var tfs = BuildTfsParameter(
            request.DepartureDate,
            request.ReturnDate,
            originCode,
            destCode);

        var cabinClassCode = GetCabinClassCode(request.CabinClass);
        var url = $"{baseUrl}/search?tfs={tfs}&hl=en&curr=USD";
        
        if (request.Passengers > 1)
            url += $"&adults={request.Passengers}";
            
        if (cabinClassCode > 0)
            url += $"&c={cabinClassCode}";

        return url;
    }

    private string BuildTfsParameter(string departureDate, string? returnDate, string origin, string dest)
    {
        // Simplified TFS encoding - in production, use proper Google Flights URL encoding
        var depDate = DateTime.Parse(departureDate);
        var tfs = $"CBwQAhopEgoyMDI2LTAxLTA5agwIAxIIL20vMGZzbXlyDQgDEgkvbS8wMl8yODY";
        
        // This is a simplified version - real implementation would need proper encoding
        return tfs;
    }

    private int GetCabinClassCode(string cabinClass)
    {
        return cabinClass.ToLower() switch
        {
            "economy" => 0,
            "premium_economy" => 1,
            "business" => 2,
            "first" => 3,
            _ => 0
        };
    }

    private async Task<List<FlightDto>> SimulateFlightSearch(FlightSearchRequest request)
    {
        await Task.Delay(100); // Simulate API call

        var random = new Random();
        var airlines = new[]
        {
            "United Airlines", "Delta", "American Airlines", "Southwest",
            "JetBlue", "Air France", "British Airways", "Lufthansa",
            "Emirates", "Qatar Airways"
        };

        var flights = new List<FlightDto>();
        var flightCount = random.Next(3, 8);

        for (int i = 0; i < flightCount; i++)
        {
            var airline = airlines[random.Next(airlines.Length)];
            var departureHour = random.Next(5, 23);
            var durationHours = random.Next(2, 15);
            var durationMinutes = random.Next(0, 60);
            var arrivalHour = (departureHour + durationHours) % 24;
            var stops = random.Next(0, 3);
            var basePrice = 200 + random.Next(100, 2000);

            flights.Add(new FlightDto
            {
                Airline = airline,
                FlightNumber = $"{GetAirlineCode(airline)}{random.Next(100, 9999)}",
                DepartureTime = $"{departureHour:D2}:{random.Next(0, 60):D2}",
                ArrivalTime = $"{arrivalHour:D2}:{random.Next(0, 60):D2}",
                Duration = $"{durationHours}h {durationMinutes}m",
                Stops = stops,
                Price = basePrice + (stops * 50),
                Currency = "USD"
            });
        }

        return flights.OrderBy(f => f.Price).ToList();
    }

    private string GetAirlineCode(string airline)
    {
        return airline switch
        {
            "United Airlines" => "UA",
            "Delta" => "DL",
            "American Airlines" => "AA",
            "Southwest" => "WN",
            "JetBlue" => "B6",
            "Air France" => "AF",
            "British Airways" => "BA",
            "Lufthansa" => "LH",
            "Emirates" => "EK",
            "Qatar Airways" => "QR",
            _ => "XX"
        };
    }
}
