using DuncanLaud.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuncanLaud.WebUI.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsDbContext _db;

    public AnalyticsController(AnalyticsDbContext db) => _db = db;

    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 1000);
        var rows = await _db.Sessions
            .OrderByDescending(s => s.SessionStart)
            .Take(limit)
            .Select(s => new
            {
                s.SessionId,
                s.IpPrefix,
                s.UserAgentRaw,
                s.SessionStart,
                s.SessionEnd,
                s.EventCount,
                s.IsBot,
            })
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("page-events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPageEvents([FromQuery] int limit = 100, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 1000);
        var rows = await _db.PageEvents
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .Select(e => new
            {
                e.PageEventId,
                e.SessionId,
                e.CreatedAt,
                e.UrlPath,
                e.UrlQuery,
                e.Referrer,
                e.HttpMethod,
                e.StatusCode,
                e.LatencyMs,
            })
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("api-events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiEvents([FromQuery] int limit = 100, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 1000);
        var rows = await _db.ApiEvents
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .Select(e => new
            {
                e.ApiEventId,
                e.SessionId,
                e.CreatedAt,
                e.Endpoint,
                e.HttpMethod,
                e.StatusCode,
                e.LatencyMs,
                e.RequestBytes,
                e.ResponseBytes,
            })
            .ToListAsync(ct);
        return Ok(rows);
    }
}
