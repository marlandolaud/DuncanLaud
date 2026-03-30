using DuncanLaud.Analytics.Services;
using DuncanLaud.WebUI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DuncanLaud.WebUI.Tests;

public class AnalyticsMiddlewareTests
{
    private static AnalyticsMiddleware Create(RequestDelegate next, IServiceScopeFactory scopeFactory)
        => new(next, scopeFactory);

    private static (
        Mock<IServiceScopeFactory> factoryMock,
        Mock<IAnalyticsService> analyticsMock,
        TaskCompletionSource<AnalyticsRequest> tcs)
        BuildMocks()
    {
        var tcs = new TaskCompletionSource<AnalyticsRequest>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var analyticsMock = new Mock<IAnalyticsService>();
        analyticsMock
            .Setup(s => s.TrackAsync(It.IsAny<AnalyticsRequest>()))
            .Callback<AnalyticsRequest>(req => tcs.TrySetResult(req))
            .Returns(Task.CompletedTask);

        var spMock = new Mock<IServiceProvider>();
        spMock.Setup(sp => sp.GetService(typeof(IAnalyticsService)))
              .Returns(analyticsMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(spMock.Object);

        var factoryMock = new Mock<IServiceScopeFactory>();
        factoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        return (factoryMock, analyticsMock, tcs);
    }

    // ── Static asset skipping ──────────────────────────────

    [Theory]
    [InlineData("/bundle.js")]
    [InlineData("/styles.css")]
    [InlineData("/logo.png")]
    [InlineData("/favicon.ico")]
    [InlineData("/font.woff2")]
    [InlineData("/sprite.svg")]
    public async Task InvokeAsync_StaticExtension_SkipsTrackingAndCallsNext(string path)
    {
        var (factoryMock, _, _) = BuildMocks();
        var nextCalled = false;
        var next = new RequestDelegate(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var middleware = Create(next, factoryMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        factoryMock.Verify(f => f.CreateScope(), Times.Never);
    }

    // ── API request tracking ───────────────────────────────

    [Fact]
    public async Task InvokeAsync_ApiPath_TracksAsApiRequest()
    {
        var (factoryMock, _, tcs) = BuildMocks();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = Create(next, factoryMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/group";

        await middleware.InvokeAsync(context);

        var tracked = await WaitForTrackAsync(tcs);
        Assert.True(tracked.IsApiRequest);
        Assert.Equal("/api/group", tracked.Path);
        Assert.Equal("POST", tracked.HttpMethod);
    }

    // ── Page request tracking ──────────────────────────────

    [Fact]
    public async Task InvokeAsync_NonApiPath_TracksAsPageRequest()
    {
        var (factoryMock, _, tcs) = BuildMocks();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = Create(next, factoryMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/home";

        await middleware.InvokeAsync(context);

        var tracked = await WaitForTrackAsync(tcs);
        Assert.False(tracked.IsApiRequest);
        Assert.Equal("/home", tracked.Path);
    }

    [Fact]
    public async Task InvokeAsync_WithQueryString_PassesQueryToTracker()
    {
        var (factoryMock, _, tcs) = BuildMocks();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = Create(next, factoryMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/search";
        context.Request.QueryString = new QueryString("?q=test");

        await middleware.InvokeAsync(context);

        var tracked = await WaitForTrackAsync(tcs);
        Assert.Equal("?q=test", tracked.QueryString);
    }

    [Fact]
    public async Task InvokeAsync_NoQueryString_PassesNullToTracker()
    {
        var (factoryMock, _, tcs) = BuildMocks();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = Create(next, factoryMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/home";
        // No query string set → default is empty

        await middleware.InvokeAsync(context);

        var tracked = await WaitForTrackAsync(tcs);
        Assert.Null(tracked.QueryString);
    }

    [Fact]
    public async Task InvokeAsync_WithReferrer_PassesReferrerToTracker()
    {
        var (factoryMock, _, tcs) = BuildMocks();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = Create(next, factoryMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/page";
        context.Request.Headers.Referer = "https://example.com";

        await middleware.InvokeAsync(context);

        var tracked = await WaitForTrackAsync(tcs);
        Assert.Equal("https://example.com", tracked.Referrer);
    }

    [Fact]
    public async Task InvokeAsync_UserAgent_PassedToTracker()
    {
        var (factoryMock, _, tcs) = BuildMocks();
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = Create(next, factoryMock.Object);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/page";
        context.Request.Headers.UserAgent = "Mozilla/5.0 Test";

        await middleware.InvokeAsync(context);

        var tracked = await WaitForTrackAsync(tcs);
        Assert.Equal("Mozilla/5.0 Test", tracked.UserAgent);
    }

    // ── Helper ─────────────────────────────────────────────

    private static async Task<AnalyticsRequest> WaitForTrackAsync(
        TaskCompletionSource<AnalyticsRequest> tcs,
        int timeoutMs = 5000)
    {
        var winner = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
        Assert.Equal(tcs.Task, winner);
        return tcs.Task.Result;
    }
}
