using DuncanLaud.WebUI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace DuncanLaud.WebUI.Tests;

public class ExceptionLoggingMiddlewareTests
{
    private static ExceptionLoggingMiddleware Create(
        RequestDelegate next,
        ILogger<ExceptionLoggingMiddleware> logger) => new(next, logger);

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextAndDoesNotLog()
    {
        var loggerMock = new Mock<ILogger<ExceptionLoggingMiddleware>>();
        var nextCalled = false;
        var next = new RequestDelegate(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var middleware = Create(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
        loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_Returns500WithCorrelationId()
    {
        var loggerMock = new Mock<ILogger<ExceptionLoggingMiddleware>>();
        var next = new RequestDelegate(_ => throw new InvalidOperationException("Boom"));

        var middleware = Create(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var json = JsonDocument.Parse(body);

        Assert.True(json.RootElement.TryGetProperty("correlationId", out var corrIdProp));
        var correlationId = corrIdProp.GetString();
        Assert.NotNull(correlationId);
        // Guid "N" format = 32 lowercase hex characters
        Assert.Equal(32, correlationId!.Length);
        Assert.Equal(correlationId, correlationId.ToLowerInvariant());
        Assert.All(correlationId, c => Assert.True("0123456789abcdef".Contains(c)));
    }

    [Fact]
    public async Task InvokeAsync_ExceptionThrown_LogsErrorWithCorrectException()
    {
        var loggerMock = new Mock<ILogger<ExceptionLoggingMiddleware>>();
        var exception = new InvalidOperationException("Test error");
        var next = new RequestDelegate(_ => throw exception);

        var middleware = Create(next, loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_MultipleExceptions_EachGetsUniqueCorrelationId()
    {
        var loggerMock = new Mock<ILogger<ExceptionLoggingMiddleware>>();
        var next = new RequestDelegate(_ => throw new Exception("err"));
        var middleware = Create(next, loggerMock.Object);

        var ids = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            using var json = JsonDocument.Parse(body);
            ids.Add(json.RootElement.GetProperty("correlationId").GetString()!);
        }

        // All correlation IDs must be distinct
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
