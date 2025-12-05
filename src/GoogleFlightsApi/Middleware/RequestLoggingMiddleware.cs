using GoogleFlightsApi.Data;
using GoogleFlightsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GoogleFlightsApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log the request
            await LogRequestAsync(
                dbContext,
                context,
                stopwatch.ElapsedMilliseconds,
                null);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            await LogRequestAsync(
                dbContext,
                context,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }

    private async Task LogRequestAsync(
        ApplicationDbContext dbContext,
        HttpContext context,
        long durationMs,
        string? errorMessage)
    {
        try
        {
            var ipAddress = GetClientIpAddress(context);
            
            // Find or create client info
            var clientInfo = await dbContext.ClientInfos
                .FirstOrDefaultAsync(c => c.IpAddress == ipAddress);

            var requestLog = new RequestLog
            {
                ClientInfoId = clientInfo?.Id,
                Method = context.Request.Method,
                Path = context.Request.Path,
                StatusCode = context.Response.StatusCode,
                DurationMs = durationMs,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = errorMessage
            };

            dbContext.RequestLogs.Add(requestLog);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Request: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - IP: {IpAddress}",
                requestLog.Method,
                requestLog.Path,
                requestLog.StatusCode,
                requestLog.DurationMs,
                ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request");
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header (proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
                return ips[0].Trim();
        }

        // Try X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        // Fall back to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
