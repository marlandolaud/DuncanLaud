using DuncanLaud.Analytics;
using DuncanLaud.Analytics.Entities;
using DuncanLaud.Analytics.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuncanLaud.Infrastructure.Tests;

public class AnalyticsServiceTests
{
    private static AnalyticsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AnalyticsDbContext(options);
    }

    // ── Static helper tests ──────────────────────────────

    [Fact]
    public void HashString_ReturnsConsistentLowercaseHex()
    {
        var result1 = AnalyticsService.HashString("hello");
        var result2 = AnalyticsService.HashString("hello");

        Assert.Equal(result1, result2);
        Assert.Equal(64, result1.Length);
        Assert.Equal(result1, result1.ToLowerInvariant());
        Assert.All(result1, c => Assert.True("0123456789abcdef".Contains(c)));
    }

    [Fact]
    public void GetIpPrefix_IPv4_MasksLastOctet()
    {
        var result = AnalyticsService.GetIpPrefix("192.168.1.55");
        Assert.Equal("192.168.1", result);
    }

    [Fact]
    public void GetIpPrefix_IPv6_KeepsFirst4Groups()
    {
        var result = AnalyticsService.GetIpPrefix("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        var parts = result.Split(':');
        Assert.Equal(4, parts.Length);
        Assert.Equal("2001", parts[0]);
    }

    [Fact]
    public void GetIpPrefix_IPv4MappedIPv6_MasksLastOctet()
    {
        var result = AnalyticsService.GetIpPrefix("::ffff:192.168.1.55");
        Assert.Equal("192.168.1", result);
    }

    [Fact]
    public void DetectBot_BotUserAgent_ReturnsTrue()
    {
        var result = AnalyticsService.DetectBot("Mozilla/5.0 (compatible; Googlebot/2.1)");
        Assert.True(result);
    }

    [Fact]
    public void DetectBot_NormalUserAgent_ReturnsFalse()
    {
        var result = AnalyticsService.DetectBot("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        Assert.False(result);
    }

    [Fact]
    public void DetectBot_NullUserAgent_ReturnsFalse()
    {
        var result = AnalyticsService.DetectBot(null);
        Assert.False(result);
    }

    // ── TrackAsync integration tests ─────────────────────

    [Fact]
    public async Task TrackAsync_NewSession_CreatesSessionAndPageEvent()
    {
        using var db = CreateDb();
        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);

        await svc.TrackAsync(new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0",
            Path: "/home",
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 50,
            IsApiRequest: false));

        Assert.Equal(1, await db.Sessions.CountAsync());
        Assert.Equal(1, await db.PageEvents.CountAsync());
    }

    [Fact]
    public async Task TrackAsync_ExistingOpenSession_ReusesSession()
    {
        using var db = CreateDb();
        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);

        var request = new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0",
            Path: "/home",
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 50,
            IsApiRequest: false);

        await svc.TrackAsync(request);
        await svc.TrackAsync(request);

        Assert.Equal(1, await db.Sessions.CountAsync());
        Assert.Equal(2, await db.PageEvents.CountAsync());
    }

    [Fact]
    public async Task TrackAsync_ApiRequest_CreatesApiEvent()
    {
        using var db = CreateDb();
        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);

        await svc.TrackAsync(new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0",
            Path: "/api/group",
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 30,
            IsApiRequest: true));

        Assert.Equal(1, await db.ApiEvents.CountAsync());
        Assert.Equal(0, await db.PageEvents.CountAsync());
    }

    [Fact]
    public async Task TrackAsync_BotRequest_FlagsSession()
    {
        using var db = CreateDb();
        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);

        await svc.TrackAsync(new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0 (compatible; Googlebot/2.1)",
            Path: "/home",
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 50,
            IsApiRequest: false));

        var session = await db.Sessions.FirstAsync();
        Assert.True(session.IsBot);
    }

    [Fact]
    public async Task TrackAsync_ExceptionInDb_DoesNotPropagate()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ThrowingAnalyticsDbContext(options);
        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);

        // Should not throw
        await svc.TrackAsync(new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0",
            Path: "/home",
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 50,
            IsApiRequest: false));
    }

    [Fact]
    public async Task TrackAsync_SessionExpired_CreatesNewSession()
    {
        using var db = CreateDb();
        // Insert an expired session manually (started 35 minutes ago)
        var expiredSession = new Session
        {
            IpHash        = AnalyticsService.HashString("1.2.3.4"),
            IpPrefix      = "1.2.3",
            UserAgentHash = AnalyticsService.HashString("Mozilla/5.0"),
            SessionStart  = DateTime.UtcNow.AddMinutes(-35),
            SessionEnd    = null,
            EventCount    = 1,
            IsBot         = false,
        };
        db.Sessions.Add(expiredSession);
        await db.SaveChangesAsync();

        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);
        await svc.TrackAsync(new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0",
            Path: "/home",
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 50,
            IsApiRequest: false));

        Assert.Equal(2, await db.Sessions.CountAsync());
    }

    [Fact]
    public async Task TrackAsync_LongPath_Truncates()
    {
        using var db = CreateDb();
        var svc = new AnalyticsService(db, NullLogger<AnalyticsService>.Instance);
        var longPath = "/" + new string('a', 1499);

        await svc.TrackAsync(new AnalyticsRequest(
            IpAddress: "1.2.3.4",
            UserAgent: "Mozilla/5.0",
            Path: longPath,
            QueryString: null,
            HttpMethod: "GET",
            Referrer: null,
            StatusCode: 200,
            LatencyMs: 50,
            IsApiRequest: false));

        var pageEvent = await db.PageEvents.FirstAsync();
        Assert.Equal(1000, pageEvent.UrlPath.Length);
    }

    // ── Helper subclass for exception test ───────────────

    private class ThrowingAnalyticsDbContext : AnalyticsDbContext
    {
        public ThrowingAnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated DB failure");
    }
}
