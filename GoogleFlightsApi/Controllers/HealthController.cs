using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace GoogleFlightsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
            service = "Google Flights MCP Server - Web API"
        });
    }

    /// <summary>
    /// Ping endpoint
    /// </summary>
    [HttpGet("ping")]
    public ActionResult<object> Ping()
    {
        return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
    }
}
