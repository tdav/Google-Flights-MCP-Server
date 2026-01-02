using GoogleFlights.Core.Models;
using GoogleFlights.Core.Services;
using GoogleFlightsApi.Models;
using GoogleFlightsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoogleFlightsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IFlightSearchService _flightSearchService;
    private readonly IClientTrackingService _clientTrackingService;
    private readonly ISearchHistoryService _searchHistoryService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(
        IFlightSearchService flightSearchService,
        IClientTrackingService clientTrackingService,
        ISearchHistoryService searchHistoryService,
        ILogger<FlightsController> logger)
    {
        _flightSearchService = flightSearchService;
        _clientTrackingService = clientTrackingService;
        _searchHistoryService = searchHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// Search for flights
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<FlightSearchResponse>> Search([FromBody] FlightSearchRequest request)
    {
        try
        {
            ValidateRequest(request);

            // Track client
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var client = await _clientTrackingService.TrackClientAsync(ipAddress, userAgent);

            // Convert API request to Core FlightData
            var flightData = ConvertToFlightData(request);

            // Perform search using Core service
            var flightResult = await _flightSearchService.SearchFlightsAsync(flightData);

            // Convert Core FlightResult to API response
            var response = ConvertToFlightSearchResponse(flightResult);

            // Save search history
            await _searchHistoryService.SaveSearchAsync(client.Id, request, response);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid search request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights");
            return StatusCode(500, new { error = "An error occurred while searching for flights" });
        }
    }

    /// <summary>
    /// Search for flights (GET method for query parameters)
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<FlightSearchResponse>> SearchGet(
        [FromQuery] string origin,
        [FromQuery] string destination,
        [FromQuery] string departureDate,
        [FromQuery] string? returnDate = null,
        [FromQuery] int passengers = 1,
        [FromQuery] string cabinClass = "economy")
    {
        var request = new FlightSearchRequest
        {
            Origin = origin,
            Destination = destination,
            DepartureDate = departureDate,
            ReturnDate = returnDate,
            Passengers = passengers,
            CabinClass = cabinClass
        };

        return await Search(request);
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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
}