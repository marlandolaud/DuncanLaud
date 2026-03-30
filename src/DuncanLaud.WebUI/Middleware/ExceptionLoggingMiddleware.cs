using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DuncanLaud.WebUI.Middleware;

/// <summary>
/// Catches every unhandled exception in the pipeline, logs it at Error level with a
/// correlation ID and full request context, then returns HTTP 500 with only the
/// correlation ID — no exception details are ever sent to the client.
/// Must be registered first in Configure() so it wraps all other middleware.
/// </summary>
public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = Guid.NewGuid().ToString("N");

            _logger.LogError(ex,
                "Unhandled exception [CorrelationId={CorrelationId}] — {Method} {Path}{QueryString}",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { correlationId });
            await context.Response.WriteAsync(body);
        }
    }
}
