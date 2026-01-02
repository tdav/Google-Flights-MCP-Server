namespace GoogleFlightsApi.Models;

public class SearchHistoryDto
{
    public int Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Passengers { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
    public string SearchUrl { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public List<FlightDto> Flights { get; set; } = new();
}

public class ClientInfoDto
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public int SearchCount { get; set; }
}