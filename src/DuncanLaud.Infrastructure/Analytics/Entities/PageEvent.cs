namespace DuncanLaud.Analytics.Entities;

public class PageEvent
{
    public long PageEventId { get; set; }
    public long SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UrlPath { get; set; } = null!;
    public string? UrlQuery { get; set; }
    public string? Referrer { get; set; }
    public string HttpMethod { get; set; } = "GET";
    public short StatusCode { get; set; }
    public int? LatencyMs { get; set; }

    public Session Session { get; set; } = null!;
}
