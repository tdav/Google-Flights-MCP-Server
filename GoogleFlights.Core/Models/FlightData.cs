namespace GoogleFlights.Core.Models;

/// <summary>
/// Represents flight search request data
/// </summary>
public class FlightData
{
    /// <summary>
    /// Origin airport code (e.g., TAS, JFK)
    /// </summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Origin Google code (e.g., /m/0fsmy)
    /// </summary>
    public string OriginCode { get; set; } = string.Empty;

    /// <summary>
    /// Destination airport code (e.g., JFK, LAX)
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Destination Google code (e.g., /m/02_286)
    /// </summary>
    public string DestinationCode { get; set; } = string.Empty;

    /// <summary>
    /// Departure date (YYYY-MM-DD format)
    /// </summary>
    public string DepartureDate { get; set; } = string.Empty;

    /// <summary>
    /// Return date (YYYY-MM-DD format, optional for one-way trips)
    /// </summary>
    public string? ReturnDate { get; set; }

    /// <summary>
    /// Passenger information
    /// </summary>
    public Passengers Passengers { get; set; } = new();

    /// <summary>
    /// Seat/cabin class
    /// </summary>
    public SeatType SeatType { get; set; } = SeatType.Economy;

    /// <summary>
    /// Trip type
    /// </summary>
    public TripType TripType { get; set; } = TripType.RoundTrip;
}
