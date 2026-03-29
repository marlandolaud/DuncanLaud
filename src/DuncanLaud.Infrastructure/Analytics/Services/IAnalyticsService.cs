namespace DuncanLaud.Analytics.Services;

public interface IAnalyticsService
{
    Task TrackAsync(AnalyticsRequest request);
}

public sealed record AnalyticsRequest(
    string IpAddress,
    string? UserAgent,
    string Path,
    string? QueryString,
    string HttpMethod,
    string? Referrer,
    int StatusCode,
    long LatencyMs,
    bool IsApiRequest,
    int? RequestBytes = null,
    int? ResponseBytes = null);
