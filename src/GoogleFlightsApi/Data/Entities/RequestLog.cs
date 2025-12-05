namespace GoogleFlightsApi.Data.Entities;

public class RequestLog
{
    public int Id { get; set; }
    public int? ClientInfoId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }

    public ClientInfo? ClientInfo { get; set; }
}
