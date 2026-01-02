namespace GoogleFlightsApi.Models;

public class FlightSearchResponse
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string DepartureDate { get; set; } = string.Empty;
    public string? ReturnDate { get; set; }
    public int Passengers { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public List<FlightDto> Flights { get; set; } = new();
    public string SearchUrl { get; set; } = string.Empty;
}

public class FlightDto
{
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int Stops { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
}
