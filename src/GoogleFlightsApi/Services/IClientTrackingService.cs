using GoogleFlightsApi.Data.Entities;

namespace GoogleFlightsApi.Services;

public interface IClientTrackingService
{
    Task<ClientInfo> TrackClientAsync(string ipAddress, string? userAgent);
}
