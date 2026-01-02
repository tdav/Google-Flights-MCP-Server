using GoogleFlights.Core.Models;
using GoogleFlights.Core.Services;
using GoogleFlightsApi.Models;

namespace GoogleFlightsApi.Services;

/// <summary>
/// Adapter service that bridges between API models and Core service
/// </summary>
public class FlightSearchService : IFlightSearchService
{
    private readonly ILogger<FlightSearchService> _logger;
    private readonly GoogleFlights.Core.Services.IFlightSearchService _coreFlightSearchService;

    public FlightSearchService(
        ILogger<FlightSearchService> logger)
    {
        _logger = logger;
        _coreFlightSearchService = new GoogleFlights.Core.Services.FlightSearchService();
    }

    public async Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request)
    {
        ValidateRequest(request);

        // Convert API request to Core FlightData
        var flightData = ConvertToFlightData(request);

        // Call Core service
        var flightResult = await _coreFlightSearchService.SearchFlightsAsync(flightData);

        // Simulate flight search - in production, integrate with real flight API
        var simulatedFlights = await SimulateFlightSearch(request);
        
        // Convert Core FlightResult to API response and add simulated flights
        var response = ConvertToFlightSearchResponse(flightResult);
        response.Flights = simulatedFlights;

        return response;
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

    private FlightData ConvertToFlightData(FlightSearchRequest request)
    {
        var seatType = ParseSeatType(request.CabinClass);
        var tripType = !string.IsNullOrWhiteSpace(request.ReturnDate) 
            ? TripType.RoundTrip 
            : TripType.OneWay;

        return new FlightData
        {
            Origin = request.Origin,
            Destination = request.Destination,
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            Passengers = new Passengers { Adults = request.Passengers },
            SeatType = seatType,
            TripType = tripType
        };
    }

    private FlightSearchResponse ConvertToFlightSearchResponse(FlightResult flightResult)
    {
        return new FlightSearchResponse
        {
            Origin = flightResult.Origin,
            Destination = flightResult.Destination,
            DepartureDate = flightResult.DepartureDate,
            ReturnDate = flightResult.ReturnDate,
            Passengers = flightResult.Passengers,
            CabinClass = flightResult.CabinClass,
            SearchUrl = flightResult.SearchUrl,
            Flights = flightResult.Flights.Select(ConvertToFlightDto).ToList()
        };
    }

    private FlightDto ConvertToFlightDto(Flight flight)
    {
        return new FlightDto
        {
            Airline = flight.Airline,
            FlightNumber = flight.FlightNumber,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Duration = flight.Duration,
            Stops = flight.Stops,
            Price = flight.Price,
            Currency = flight.Currency
        };
    }

    private SeatType ParseSeatType(string cabinClass)
    {
        return cabinClass.ToLower() switch
        {
            "economy" => SeatType.Economy,
            "premium_economy" => SeatType.PremiumEconomy,
            "business" => SeatType.Business,
            "first" => SeatType.First,
            _ => SeatType.Economy
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
