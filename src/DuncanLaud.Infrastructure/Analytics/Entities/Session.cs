namespace DuncanLaud.Analytics.Entities;

public class Session
{
    public long SessionId { get; set; }
    public string IpHash { get; set; } = null!;
    public string IpPrefix { get; set; } = null!;
    public string UserAgentHash { get; set; } = null!;
    public string? UserAgentRaw { get; set; }
    public DateTime SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    public int EventCount { get; set; }
    public bool IsBot { get; set; }

    public ICollection<PageEvent> PageEvents { get; set; } = [];
    public ICollection<ApiEvent> ApiEvents { get; set; } = [];
}
