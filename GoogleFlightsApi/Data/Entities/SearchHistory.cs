namespace GoogleFlightsApi.Data.Entities;

public class SearchHistory
{
    public int Id { get; set; }
    public int ClientInfoId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Passengers { get; set; }
    public string CabinClass { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
    public string SearchUrl { get; set; } = string.Empty;

    public ClientInfo ClientInfo { get; set; } = null!;
    public ICollection<FlightResult> FlightResults { get; set; } = new List<FlightResult>();
}
