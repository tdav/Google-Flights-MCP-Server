using GoogleFlightsApi.Models;
using GoogleFlightsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoogleFlightsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly ISearchHistoryService _searchHistoryService;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(
        ISearchHistoryService searchHistoryService,
        ILogger<HistoryController> logger)
    {
        _searchHistoryService = searchHistoryService;
        _logger = logger;
    }

    /// <summary>
    /// Get search history for current client IP
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<SearchHistoryDto>>> GetMyHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var history = await _searchHistoryService.GetSearchHistoryAsync(ipAddress, pageNumber, pageSize);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving search history");
            return StatusCode(500, new { error = "An error occurred while retrieving search history" });
        }
    }

    /// <summary>
    /// Get all search history (admin endpoint)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<SearchHistoryDto>>> GetAllHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var history = await _searchHistoryService.GetAllSearchHistoryAsync(pageNumber, pageSize);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all search history");
            return StatusCode(500, new { error = "An error occurred while retrieving search history" });
        }
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
