using GoogleFlightsApi.Models;

namespace GoogleFlightsApi.Services;

public interface IFlightSearchService
{
    Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request);
}
