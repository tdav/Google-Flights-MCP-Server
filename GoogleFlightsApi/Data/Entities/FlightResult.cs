namespace GoogleFlightsApi.Data.Entities;

public class FlightResult
{
    public int Id { get; set; }
    public int SearchHistoryId { get; set; }
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int Stops { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;

    public SearchHistory SearchHistory { get; set; } = null!;
}
