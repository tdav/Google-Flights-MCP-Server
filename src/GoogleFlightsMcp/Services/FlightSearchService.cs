using GoogleFlightsMcp.Models;
using GoogleFlightsMcp.Helpers;
using Serilog;

namespace GoogleFlightsMcp.Services;

/// <summary>
/// Service for searching flights on Google Flights
/// </summary>
public class FlightSearchService : IFlightSearchService
{
    private static readonly Dictionary<string, string> AirportCodes = new()
    {
        // North America
        { "JFK", "/m/02_286" },
        { "LAX", "/m/030qb3t" },
        { "ORD", "/m/01_d4" },
        { "DFW", "/m/030k2v" },
        { "SFO", "/m/0d6lp" },
        { "SEA", "/m/0d9jr" },
        { "MIA", "/m/0f2v0" },
        { "BOS", "/m/01cx_" },
        { "ATL", "/m/013yq" },
        { "LAS", "/m/0cv3w" },
        { "PHX", "/m/0d35y" },
        { "DEN", "/m/02cft" },
        { "IAH", "/m/03ksg" },
        { "MSP", "/m/0fpzwf" },
        { "DTW", "/m/0fvwg" },
        { "EWR", "/m/0cc56" },
        { "MCO", "/m/0fxmq" },
        
        // Europe
        { "LHR", "/m/04jpl" },
        { "CDG", "/m/05qtj" },
        { "FRA", "/m/0jxgx" },
        { "AMS", "/m/0k3p" },
        { "MAD", "/m/056_y" },
        { "BCN", "/m/01f62" },
        { "FCO", "/m/06c62" },
        { "MUC", "/m/0727_" },
        { "IST", "/m/09949" },
        { "LGW", "/m/065y4w7" },
        { "ZRH", "/m/08g5vq" },
        { "VIE", "/m/05qx6" },
        
        // Asia
        { "DXB", "/m/01f08r" },
        { "HKG", "/m/03h64r" },
        { "NRT", "/m/0f4t4" },
        { "SIN", "/m/02p24c" },
        { "ICN", "/m/0cyzn" },
        { "BKK", "/m/0dl9t8" },
        { "DEL", "/m/03l8mx" },
        { "PEK", "/m/0dq_7" },
        { "PVG", "/m/0j5nb" },
        { "HND", "/m/0gx_x" },
        
        // Middle East & Central Asia
        { "TAS", "/m/0fsmy" },  // Tashkent
        { "DOH", "/m/01_8q3" },
        
        // Australia & Oceania
        { "SYD", "/m/06y57" },
        { "MEL", "/m/0chghy" },
        { "AKL", "/m/0ctyv" },
        
        // South America
        { "GRU", "/m/0fphj" },
        { "EZE", "/m/0132jd" },
        { "GIG", "/m/02k0l1" },
        
        // Africa
        { "JNB", "/m/04g8v" },
        { "CAI", "/m/0cwd8" },
    };

    public async Task<FlightResult> SearchFlightsAsync(FlightData flightData)
    {
        ValidateFlightData(flightData);

        // Build search URL
        var searchUrl = BuildGoogleFlightsUrl(flightData);
        
        Log.Information(
            "Searching flights: {Origin} -> {Destination} on {DepartureDate}",
            flightData.Origin, 
            flightData.Destination, 
            flightData.DepartureDate);

        // Simulate flight search (in production, this would scrape Google Flights or use an API)
        var flights = await SimulateFlightSearchAsync(flightData);

        return new FlightResult
        {
            Origin = flightData.Origin,
            Destination = flightData.Destination,
            DepartureDate = flightData.DepartureDate,
            ReturnDate = flightData.ReturnDate,
            Passengers = flightData.Passengers.Total,
            CabinClass = GetCabinClassName(flightData.SeatType),
            Flights = flights,
            SearchUrl = searchUrl,
            TripType = flightData.TripType,
            SearchedAt = DateTime.UtcNow
        };
    }

    public string BuildGoogleFlightsUrl(FlightData flightData)
    {
        // Get Google codes for airports
        var originCode = GetGoogleCode(flightData.Origin);
        var destCode = GetGoogleCode(flightData.Destination);

        // Base URL
        var baseUrl = "https://www.google.ca/travel/flights";
        
        // Build TFS parameter based on trip type
        string tfsParam;
        if (flightData.TripType == TripType.RoundTrip && !string.IsNullOrEmpty(flightData.ReturnDate))
        {
            tfsParam = BuildRoundTripTfsParameter(
                flightData.DepartureDate,
                flightData.ReturnDate!,
                originCode,
                destCode);
        }
        else
        {
            tfsParam = BuildOneWayTfsParameter(
                flightData.DepartureDate,
                originCode,
                destCode);
        }

        // Build URL with parameters
        var url = $"{baseUrl}/search?tfs={tfsParam}&hl=en&curr=USD";
        
        // Add passenger count if more than 1
        if (flightData.Passengers.Total > 1)
        {
            url += $"&adults={flightData.Passengers.Adults}";
            if (flightData.Passengers.Children > 0)
                url += $"&children={flightData.Passengers.Children}";
            if (flightData.Passengers.Infants > 0)
                url += $"&infants={flightData.Passengers.Infants}";
        }
        
        // Add cabin class if not economy
        var cabinClassCode = (int)flightData.SeatType;
        if (cabinClassCode > 0)
            url += $"&c={cabinClassCode}";

        return url;
    }

    public bool ValidateFlightData(FlightData flightData)
    {
        if (string.IsNullOrWhiteSpace(flightData.Origin))
            throw new ArgumentException("Origin airport is required");

        if (string.IsNullOrWhiteSpace(flightData.Destination))
            throw new ArgumentException("Destination airport is required");

        if (flightData.Origin.Equals(flightData.Destination, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Origin and destination must be different");

        if (string.IsNullOrWhiteSpace(flightData.DepartureDate))
            throw new ArgumentException("Departure date is required");

        if (!DateHelper.IsValidDate(flightData.DepartureDate))
            throw new ArgumentException("Departure date must be in YYYY-MM-DD format");

        if (!DateHelper.IsInFuture(flightData.DepartureDate))
            throw new ArgumentException("Departure date must be in the future or today");

        if (flightData.TripType == TripType.RoundTrip)
        {
            if (string.IsNullOrWhiteSpace(flightData.ReturnDate))
                throw new ArgumentException("Return date is required for round trip");

            if (!DateHelper.IsValidDate(flightData.ReturnDate))
                throw new ArgumentException("Return date must be in YYYY-MM-DD format");

            if (!DateHelper.IsReturnDateValid(flightData.DepartureDate, flightData.ReturnDate))
                throw new ArgumentException("Return date must be after or equal to departure date");
        }

        if (!flightData.Passengers.IsValid())
            throw new ArgumentException("Invalid passenger count");

        return true;
    }

    private string GetGoogleCode(string airportCode)
    {
        var code = airportCode.ToUpper();
        if (AirportCodes.TryGetValue(code, out var googleCode))
        {
            return googleCode;
        }
        
        // Log warning for unsupported airport code
        Log.Warning("Airport code {Code} not found in database, using fallback format", code);
        
        // Fallback format - may not always work but provides basic functionality
        return $"/m/{code.ToLower()}";
    }

    private string BuildRoundTripTfsParameter(string departureDate, string returnDate, string origin, string dest)
    {
        // This is a simplified TFS encoding
        // Real implementation would need proper Google Flights URL encoding
        // For now, we return a basic format that Google Flights can understand
        // In production, this would use proper base64 encoding of protobuf data
        
        // Note: The actual TFS parameter is complex and requires protobuf encoding
        // This is a placeholder that demonstrates the structure
        var tfs = $"CBwQARIK{departureDate.Replace("-", "")}agwIAxII{origin}cg0IAxIJ{dest}";
        return tfs;
    }

    private string BuildOneWayTfsParameter(string departureDate, string origin, string dest)
    {
        // Simplified one-way TFS encoding
        // In production, this would use proper protobuf encoding
        var tfs = $"CBwQARIK{departureDate.Replace("-", "")}agwIAxII{origin}cg0IAxIJ{dest}";
        return tfs;
    }

    private async Task<List<Flight>> SimulateFlightSearchAsync(FlightData flightData)
    {
        await Task.Delay(100); // Simulate API call

        var random = new Random();
        var airlines = new[]
        {
            "United Airlines", "Delta", "American Airlines", "Southwest",
            "JetBlue", "Air France", "British Airways", "Lufthansa",
            "Emirates", "Qatar Airways", "Turkish Airlines", "Uzbekistan Airways"
        };

        var flights = new List<Flight>();
        var flightCount = random.Next(3, 8);

        for (int i = 0; i < flightCount; i++)
        {
            var airline = airlines[random.Next(airlines.Length)];
            var departureHour = random.Next(5, 23);
            var durationHours = random.Next(2, 15);
            var durationMinutes = random.Next(0, 60);
            var arrivalHour = (departureHour + durationHours) % 24;
            var stops = random.Next(0, 3);
            
            // Base price varies by distance and cabin class
            var basePrice = 200 + random.Next(100, 2000);
            var cabinMultiplier = GetCabinPriceMultiplier(flightData.SeatType);
            var price = basePrice * cabinMultiplier + (stops * 50);

            flights.Add(new Flight
            {
                Airline = airline,
                FlightNumber = $"{GetAirlineCode(airline)}{random.Next(100, 9999)}",
                DepartureTime = $"{departureHour:D2}:{random.Next(0, 60):D2}",
                ArrivalTime = $"{arrivalHour:D2}:{random.Next(0, 60):D2}",
                Duration = $"{durationHours}h {durationMinutes}m",
                Stops = stops,
                Price = PriceHelper.RoundPrice(price, "USD"),
                Currency = "USD",
                Origin = flightData.Origin,
                Destination = flightData.Destination,
                DepartureDate = flightData.DepartureDate
            });
        }

        return flights.OrderBy(f => f.Price).ToList();
    }

    private decimal GetCabinPriceMultiplier(SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Economy => 1.0m,
            SeatType.PremiumEconomy => 1.5m,
            SeatType.Business => 3.0m,
            SeatType.First => 5.0m,
            _ => 1.0m
        };
    }

    private string GetCabinClassName(SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Economy => "economy",
            SeatType.PremiumEconomy => "premium_economy",
            SeatType.Business => "business",
            SeatType.First => "first",
            _ => "economy"
        };
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
            "Turkish Airlines" => "TK",
            "Uzbekistan Airways" => "HY",
            _ => "XX"
        };
    }
}
