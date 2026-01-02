using GoogleFlights.Core.Models;

namespace GoogleFlights.Core.Services;

/// <summary>
/// Interface for flight search service
/// </summary>
public interface IFlightSearchService
{
    /// <summary>
    /// Searches for flights based on provided flight data
    /// </summary>
    /// <param name="flightData">Flight search parameters</param>
    /// <returns>Flight search results</returns>
    Task<FlightResult> SearchFlightsAsync(FlightData flightData);

    /// <summary>
    /// Builds Google Flights URL from flight data
    /// </summary>
    /// <param name="flightData">Flight search parameters</param>
    /// <returns>Google Flights URL</returns>
    string BuildGoogleFlightsUrl(FlightData flightData);

    /// <summary>
    /// Validates flight search parameters
    /// </summary>
    /// <param name="flightData">Flight search parameters</param>
    /// <returns>True if valid, throws exception otherwise</returns>
    bool ValidateFlightData(FlightData flightData);
}
