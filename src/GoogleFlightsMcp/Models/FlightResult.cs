namespace GoogleFlightsMcp.Models;

/// <summary>
/// Represents the result of a flight search
/// </summary>
public class FlightResult
{
    /// <summary>
    /// Origin airport code
    /// </summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Destination airport code
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Departure date
    /// </summary>
    public string DepartureDate { get; set; } = string.Empty;

    /// <summary>
    /// Return date (if round trip)
    /// </summary>
    public string? ReturnDate { get; set; }

    /// <summary>
    /// Number of passengers
    /// </summary>
    public int Passengers { get; set; } = 1;

    /// <summary>
    /// Cabin class
    /// </summary>
    public string CabinClass { get; set; } = "economy";

    /// <summary>
    /// List of available flights
    /// </summary>
    public List<Flight> Flights { get; set; } = new();

    /// <summary>
    /// Google Flights search URL
    /// </summary>
    public string SearchUrl { get; set; } = string.Empty;

    /// <summary>
    /// Search timestamp
    /// </summary>
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trip type
    /// </summary>
    public TripType TripType { get; set; } = TripType.RoundTrip;
}
