namespace GoogleFlights.Core.Models;

/// <summary>
/// Represents a single flight option
/// </summary>
public class Flight
{
    /// <summary>
    /// Airline name
    /// </summary>
    public string Airline { get; set; } = string.Empty;

    /// <summary>
    /// Flight number
    /// </summary>
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Departure time (HH:mm format)
    /// </summary>
    public string DepartureTime { get; set; } = string.Empty;

    /// <summary>
    /// Arrival time (HH:mm format)
    /// </summary>
    public string ArrivalTime { get; set; } = string.Empty;

    /// <summary>
    /// Flight duration
    /// </summary>
    public string Duration { get; set; } = string.Empty;

    /// <summary>
    /// Number of stops
    /// </summary>
    public int Stops { get; set; }

    /// <summary>
    /// Price per person
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, UZS)
    /// </summary>
    public string Currency { get; set; } = "USD";

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
}
