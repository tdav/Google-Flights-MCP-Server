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
    
    /// <summary>
    /// All flight results stored as a JSONB array
    /// </summary>
    public string FlightsJson { get; set; } = "[]";

    public ClientInfo ClientInfo { get; set; } = null!;
}