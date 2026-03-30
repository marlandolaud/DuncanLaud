using DuncanLaud.Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace DuncanLaud.Analytics.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AnalyticsDbContext _db;
    private readonly ILogger<AnalyticsService> logger;

    public AnalyticsService(AnalyticsDbContext db, ILogger<AnalyticsService> logger)
    {
        _db = db;
        this.logger = logger;
    }

    private static readonly string[] BotKeywords =
    [
        "bot", "crawler", "spider", "scraper", "wget", "curl/", "python-requests",
        "facebookexternalhit", "ia_archiver", "semrushbot", "ahrefsbot", "mj12bot"
    ];

    public async Task TrackAsync(AnalyticsRequest request)
    {
        string ipPrefix = null;
        try
        {
            var ipHash    = HashString(request.IpAddress);
            ipPrefix  = GetIpPrefix(request.IpAddress);
            var uaHash    = HashString(request.UserAgent ?? string.Empty);
            var isBot     = DetectBot(request.UserAgent);
            var now       = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-30);

            // Match any session whose last activity (SessionEnd if set, else SessionStart)
            // falls within the 30-minute inactivity window.
            var session = await _db.Sessions
                .Where(s => s.IpHash == ipHash
                    && (s.SessionEnd >= windowStart
                        || (s.SessionEnd == null && s.SessionStart >= windowStart)))
                .OrderByDescending(s => s.SessionEnd ?? s.SessionStart)
                .FirstOrDefaultAsync();

            if (session is null)
            {
                session = new Session
                {
                    IpHash        = ipHash,
                    IpPrefix      = ipPrefix,
                    UserAgentHash = uaHash,
                    UserAgentRaw  = request.UserAgent is { Length: > 512 } ua ? ua[..512] : request.UserAgent,
                    SessionStart  = now,
                    EventCount    = 0,
                    IsBot         = isBot,
                };
                _db.Sessions.Add(session);
            }
            else
            {
                session.SessionEnd = now;
                session.EventCount++;
            }

            await _db.SaveChangesAsync();

            if (request.IsApiRequest)
            {
                _db.ApiEvents.Add(new ApiEvent
                {
                    SessionId     = session.SessionId,
                    CreatedAt     = now,
                    Endpoint      = request.Path.Length > 1000 ? request.Path[..1000] : request.Path,
                    HttpMethod    = request.HttpMethod,
                    StatusCode    = (short)request.StatusCode,
                    LatencyMs     = request.LatencyMs > int.MaxValue ? int.MaxValue : (int)request.LatencyMs,
                    RequestBytes  = request.RequestBytes,
                    ResponseBytes = request.ResponseBytes,
                });
            }
            else
            {
                _db.PageEvents.Add(new PageEvent
                {
                    SessionId  = session.SessionId,
                    CreatedAt  = now,
                    UrlPath    = request.Path.Length > 1000 ? request.Path[..1000] : request.Path,
                    UrlQuery   = request.QueryString is { Length: > 2000 } q ? q[..2000] : request.QueryString,
                    Referrer   = request.Referrer is { Length: > 2000 } r ? r[..2000] : request.Referrer,
                    HttpMethod = request.HttpMethod,
                    StatusCode = (short)request.StatusCode,
                    LatencyMs  = request.LatencyMs > int.MaxValue ? int.MaxValue : (int)request.LatencyMs,
                });
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
                logger.LogError(ex,
                    "Failure to capture analytics {IP}",
                    ipPrefix);
        }
    }

    public static string HashString(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string GetIpPrefix(string ip)
    {
        if (!IPAddress.TryParse(ip, out var addr))
            return ip.Length > 16 ? ip[..16] : ip;

        if (addr.IsIPv4MappedToIPv6)
            addr = addr.MapToIPv4();

        if (addr.AddressFamily == AddressFamily.InterNetwork)
        {
            var parts = addr.ToString().Split('.');
            return parts.Length == 4 ? string.Join('.', parts[..3]) : ip;
        }

        if (addr.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var parts = addr.ToString().Split(':');
            return string.Join(':', parts.Take(4));
        }

        return ip.Length > 16 ? ip[..16] : ip;
    }

    public static bool DetectBot(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return false;
        var ua = userAgent.ToLowerInvariant();
        return BotKeywords.Any(k => ua.Contains(k));
    }
}
