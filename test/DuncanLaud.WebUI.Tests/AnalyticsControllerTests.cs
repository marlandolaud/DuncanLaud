using DuncanLaud.Analytics;
using DuncanLaud.Analytics.Entities;
using DuncanLaud.WebUI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.WebUI.Tests;

public class AnalyticsControllerTests
{
    private static AnalyticsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AnalyticsDbContext(options);
    }

    private static Session MakeSession(DateTime? sessionStart = null) => new()
    {
        IpHash        = "abc123",
        IpPrefix      = "1.2.3",
        UserAgentHash = "def456",
        SessionStart  = sessionStart ?? DateTime.UtcNow,
        EventCount    = 0,
        IsBot         = false,
    };

    [Fact]
    public async Task GetSessions_ReturnsOk_WithSessionData()
    {
        using var db = CreateDb();
        db.Sessions.AddRange(MakeSession(), MakeSession(), MakeSession());
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetSessions(limit: 10);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        Assert.Equal(3, list.Cast<object>().Count());
    }

    [Fact]
    public async Task GetSessions_LimitClamped_ToMax1000()
    {
        using var db = CreateDb();
        db.Sessions.AddRange(MakeSession(), MakeSession());
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetSessions(limit: 9999);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetSessions_LimitClamped_ToMin1()
    {
        using var db = CreateDb();
        db.Sessions.Add(MakeSession());
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetSessions(limit: 0);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetSessions_ReturnsOrderedByMostRecent()
    {
        using var db = CreateDb();
        var older = MakeSession(DateTime.UtcNow.AddHours(-2));
        var newer = MakeSession(DateTime.UtcNow.AddHours(-1));
        db.Sessions.Add(older);
        db.Sessions.Add(newer);
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetSessions(limit: 10);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = ok.Value as System.Collections.IEnumerable;
        Assert.NotNull(list);

        // Get the sessionStart values via reflection on the anonymous type
        var items = list!.Cast<object>().ToList();
        Assert.Equal(2, items.Count);

        var firstStart  = (DateTime)items[0].GetType().GetProperty("SessionStart")!.GetValue(items[0])!;
        var secondStart = (DateTime)items[1].GetType().GetProperty("SessionStart")!.GetValue(items[1])!;
        Assert.True(firstStart >= secondStart);
    }

    [Fact]
    public async Task GetPageEvents_ReturnsOk()
    {
        using var db = CreateDb();
        var session = MakeSession();
        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        db.PageEvents.Add(new PageEvent
        {
            SessionId  = session.SessionId,
            CreatedAt  = DateTime.UtcNow,
            UrlPath    = "/home",
            HttpMethod = "GET",
            StatusCode = 200,
        });
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetPageEvents(limit: 10);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task GetApiEvents_ReturnsOk()
    {
        using var db = CreateDb();
        var session = MakeSession();
        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        db.ApiEvents.Add(new ApiEvent
        {
            SessionId  = session.SessionId,
            CreatedAt  = DateTime.UtcNow,
            Endpoint   = "/api/group",
            HttpMethod = "GET",
            StatusCode = 200,
        });
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetApiEvents(limit: 10);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task GetPageEvents_RespectsLimit()
    {
        using var db = CreateDb();
        var session = MakeSession();
        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            db.PageEvents.Add(new PageEvent
            {
                SessionId  = session.SessionId,
                CreatedAt  = DateTime.UtcNow,
                UrlPath    = $"/page{i}",
                HttpMethod = "GET",
                StatusCode = 200,
            });
        }
        await db.SaveChangesAsync();

        var ctrl = new AnalyticsController(db);
        var result = await ctrl.GetPageEvents(limit: 3);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        Assert.Equal(3, list.Cast<object>().Count());
    }
}
