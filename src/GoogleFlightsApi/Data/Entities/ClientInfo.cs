namespace GoogleFlightsApi.Data.Entities;

public class ClientInfo
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public int SearchCount { get; set; }

    public ICollection<SearchHistory> Searches { get; set; } = new List<SearchHistory>();
    public ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();
}
