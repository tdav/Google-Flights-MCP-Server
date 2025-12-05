using GoogleFlightsApi.Data;
using GoogleFlightsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoogleFlightsApi.Services;

public class ClientTrackingService : IClientTrackingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientTrackingService> _logger;

    public ClientTrackingService(
        ApplicationDbContext context,
        ILogger<ClientTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClientInfo> TrackClientAsync(string ipAddress, string? userAgent)
    {
        var client = await _context.ClientInfos
            .FirstOrDefaultAsync(c => c.IpAddress == ipAddress);

        if (client == null)
        {
            client = new ClientInfo
            {
                IpAddress = ipAddress,
                UserAgent = userAgent,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                SearchCount = 0
            };

            _context.ClientInfos.Add(client);
            _logger.LogInformation("New client tracked: {IpAddress}", ipAddress);
        }
        else
        {
            client.LastSeen = DateTime.UtcNow;
            client.UserAgent = userAgent ?? client.UserAgent;
            _context.ClientInfos.Update(client);
            _logger.LogDebug("Client updated: {IpAddress}", ipAddress);
        }

        await _context.SaveChangesAsync();
        return client;
    }
}
