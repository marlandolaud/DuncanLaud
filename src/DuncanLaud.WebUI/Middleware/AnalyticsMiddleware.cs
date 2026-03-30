using DuncanLaud.Analytics.Services;
using System.Diagnostics;

namespace DuncanLaud.WebUI.Middleware;

public class AnalyticsMiddleware
{
    private static readonly HashSet<string> StaticExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico",
        ".woff", ".woff2", ".ttf", ".eot", ".map", ".webp", ".txt", ".xml"
    };

    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public AnalyticsMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ext = Path.GetExtension(context.Request.Path.Value);
        if (!string.IsNullOrEmpty(ext) && StaticExtensions.Contains(ext))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var requestBytes = context.Request.ContentLength;
        await _next(context);
        sw.Stop();

        var ip        = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua        = context.Request.Headers.UserAgent.ToString();
        var path      = context.Request.Path.Value ?? "/";
        var query     = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null;
        var referrer  = context.Request.Headers.Referer.ToString();
        var method    = context.Request.Method;
        var status    = context.Response.StatusCode;
        var latency   = sw.ElapsedMilliseconds;
        var isApi     = context.Request.Path.StartsWithSegments("/api");
        var respBytes = context.Response.ContentLength;

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
            await analytics.TrackAsync(new AnalyticsRequest(
                IpAddress:     ip,
                UserAgent:     string.IsNullOrEmpty(ua) ? null : ua,
                Path:          path,
                QueryString:   query,
                HttpMethod:    method,
                Referrer:      string.IsNullOrEmpty(referrer) ? null : referrer,
                StatusCode:    status,
                LatencyMs:     latency,
                IsApiRequest:  isApi,
                RequestBytes:  requestBytes.HasValue ? (int?)requestBytes.Value : null,
                ResponseBytes: respBytes.HasValue ? (int?)respBytes.Value : null));
        });
    }
}
