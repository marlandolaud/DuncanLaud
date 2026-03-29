namespace DuncanLaud.Analytics.Entities;

public class ApiEvent
{
    public long ApiEventId { get; set; }
    public long SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Endpoint { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public short StatusCode { get; set; }
    public int? LatencyMs { get; set; }
    public int? RequestBytes { get; set; }
    public int? ResponseBytes { get; set; }

    public Session Session { get; set; } = null!;
}
