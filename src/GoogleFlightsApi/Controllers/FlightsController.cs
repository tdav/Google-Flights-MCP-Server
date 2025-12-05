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
            // Track client
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();
            var client = await _clientTrackingService.TrackClientAsync(ipAddress, userAgent);

            // Perform search
            var response = await _flightSearchService.SearchFlightsAsync(request);

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
}
