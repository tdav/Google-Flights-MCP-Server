using GoogleFlightsApi.Models;

namespace GoogleFlightsApi.Services;

/// <summary>
/// Interface for flight search service
/// </summary>
public interface IFlightSearchService
{
    /// <summary>
    /// Searches for flights based on the provided request
    /// </summary>
    /// <param name="request">Flight search request</param>
    /// <returns>Flight search response with results</returns>
    Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request);
}
