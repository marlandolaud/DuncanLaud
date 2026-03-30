# Website Analytics — Design Document

## Overview

Cookieless, server-side analytics system tracking page loads and API calls, correlated by IP address. No client-side scripts, no third-party tools, no cookies. All tracking is implemented as server middleware writing asynchronously to a dedicated MSSQL database.

---

## Requirements

### Functional

- Log an event on every page load (server-side only)
- Log every subsequent API call made within an active session
- Correlate page loads and API calls to a session via IP address
- Session window: 30 minutes of inactivity closes a session
- Handle shared IPs (NAT/proxies) on a best-effort basis — correlation is not guaranteed to be 1:1 per user

### Non-functional

- No cookies set on the client, ever
- IP addresses stored as SHA-256 hash only; last octet masked for any human-readable storage
- Logging must be fully asynchronous — must not block the HTTP response
- Target overhead: < 5ms per tracked request
- Data retention: configurable, default 90 days
- Privacy policy must document IP-based tracking

---

## Database

### Provider
- **MSSQL** via **Entity Framework Core**
- Schema name: `analytics`
- Migrations managed with `dotnet ef`

### Schema diagram

```
analytics.sessions
    session_id (PK, BIGINT, IDENTITY)
    ip_hash (NVARCHAR 64)          -- SHA-256 of full IP
    ip_prefix (NVARCHAR 16)        -- e.g. "192.168.1" (last octet masked)
    user_agent_hash (NVARCHAR 64)  -- SHA-256 of user agent string
    user_agent_raw (NVARCHAR 512)  -- nullable, for debugging
    session_start (DATETIME2(3))
    session_end (DATETIME2(3))     -- nullable, updated on last activity
    event_count (INT)              -- default 0
    is_bot (BIT)                   -- default false

analytics.page_events
    page_event_id (PK, BIGINT, IDENTITY)
    session_id (FK -> sessions)
    created_at (DATETIME2(3))      -- default SYSUTCDATETIME()
    url_path (NVARCHAR 1000)
    url_query (NVARCHAR 2000)      -- nullable
    referrer (NVARCHAR 2000)       -- nullable
    http_method (NVARCHAR 10)      -- default "GET"
    status_code (SMALLINT)
    latency_ms (INT)               -- nullable

analytics.api_events
    api_event_id (PK, BIGINT, IDENTITY)
    session_id (FK -> sessions)
    created_at (DATETIME2(3))      -- default SYSUTCDATETIME()
    endpoint (NVARCHAR 1000)
    http_method (NVARCHAR 10)
    status_code (SMALLINT)
    latency_ms (INT)               -- nullable
    request_bytes (INT)            -- nullable
    response_bytes (INT)           -- nullable
```

### Indexes

| Index name | Table | Columns |
|---|---|---|
| `IX_sessions_ip_hash` | `sessions` | `ip_hash`, `session_start` |
| `IX_page_events_session` | `page_events` | `session_id`, `created_at` |
| `IX_page_events_path` | `page_events` | `url_path`, `created_at` |
| `IX_api_events_session` | `api_events` | `session_id`, `created_at` |
| `IX_api_events_endpoint` | `api_events` | `endpoint`, `created_at` |

---

## Project structure

```
YourApp/
├── Analytics/
│   ├── Entities/
│   │   ├── Session.cs
│   │   ├── PageEvent.cs
│   │   └── ApiEvent.cs
│   ├── Configuration/
│   │   ├── SessionConfiguration.cs
│   │   ├── PageEventConfiguration.cs
│   │   └── ApiEventConfiguration.cs
│   ├── Middleware/
│   │   └── AnalyticsMiddleware.cs
│   ├── Services/
│   │   └── AnalyticsService.cs
│   └── AnalyticsDbContext.cs
```

---

## EF Core entities

### `Session.cs`

```csharp
namespace YourApp.Analytics.Entities;

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
```

### `PageEvent.cs`

```csharp
namespace YourApp.Analytics.Entities;

public class PageEvent
{
    public long PageEventId { get; set; }
    public long SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UrlPath { get; set; } = null!;
    public string? UrlQuery { get; set; }
    public string? Referrer { get; set; }
    public string HttpMethod { get; set; } = "GET";
    public short StatusCode { get; set; }
    public int? LatencyMs { get; set; }

    public Session Session { get; set; } = null!;
}
```

### `ApiEvent.cs`

```csharp
namespace YourApp.Analytics.Entities;

public class ApiEvent
{
    public long ApiEventId { get; set; }
    public long SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Endpoint { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public short StatusCode { get; set; }
    public int? LatencyMs { get; set; }
    public int? RequestBytes { get; set; }
    public int? ResponseBytes { get; set; }

    public Session Session { get; set; } = null!;
}
```

---

## EF Core configuration

### `AnalyticsDbContext.cs`

```csharp
namespace YourApp.Analytics;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<PageEvent> PageEvents => Set<PageEvent>();
    public DbSet<ApiEvent> ApiEvents => Set<ApiEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analytics");

        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        modelBuilder.ApplyConfiguration(new PageEventConfiguration());
        modelBuilder.ApplyConfiguration(new ApiEventConfiguration());
    }
}
```

### `SessionConfiguration.cs`

```csharp
namespace YourApp.Analytics.Configuration;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(s => s.SessionId);
        builder.Property(s => s.SessionId).UseIdentityColumn();

        builder.Property(s => s.IpHash).HasMaxLength(64).IsRequired();
        builder.Property(s => s.IpPrefix).HasMaxLength(16).IsRequired();
        builder.Property(s => s.UserAgentHash).HasMaxLength(64).IsRequired();
        builder.Property(s => s.UserAgentRaw).HasMaxLength(512);
        builder.Property(s => s.SessionStart).HasColumnType("datetime2(3)").IsRequired();
        builder.Property(s => s.SessionEnd).HasColumnType("datetime2(3)");
        builder.Property(s => s.EventCount).HasDefaultValue(0).IsRequired();
        builder.Property(s => s.IsBot).HasDefaultValue(false).IsRequired();

        builder.HasIndex(s => new { s.IpHash, s.SessionStart })
            .HasDatabaseName("IX_sessions_ip_hash");
    }
}
```

### `PageEventConfiguration.cs`

```csharp
namespace YourApp.Analytics.Configuration;

public class PageEventConfiguration : IEntityTypeConfiguration<PageEvent>
{
    public void Configure(EntityTypeBuilder<PageEvent> builder)
    {
        builder.ToTable("page_events");

        builder.HasKey(e => e.PageEventId);
        builder.Property(e => e.PageEventId).UseIdentityColumn();

        builder.Property(e => e.UrlPath).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.UrlQuery).HasMaxLength(2000);
        builder.Property(e => e.Referrer).HasMaxLength(2000);
        builder.Property(e => e.HttpMethod).HasMaxLength(10).HasDefaultValue("GET").IsRequired();
        builder.Property(e => e.StatusCode).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(e => e.Session)
            .WithMany(s => s.PageEvents)
            .HasForeignKey(e => e.SessionId);

        builder.HasIndex(e => new { e.SessionId, e.CreatedAt })
            .HasDatabaseName("IX_page_events_session");
        builder.HasIndex(e => new { e.UrlPath, e.CreatedAt })
            .HasDatabaseName("IX_page_events_path");
    }
}
```

### `ApiEventConfiguration.cs`

```csharp
namespace YourApp.Analytics.Configuration;

public class ApiEventConfiguration : IEntityTypeConfiguration<ApiEvent>
{
    public void Configure(EntityTypeBuilder<ApiEvent> builder)
    {
        builder.ToTable("api_events");

        builder.HasKey(e => e.ApiEventId);
        builder.Property(e => e.ApiEventId).UseIdentityColumn();

        builder.Property(e => e.Endpoint).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.HttpMethod).HasMaxLength(10).IsRequired();
        builder.Property(e => e.StatusCode).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(e => e.Session)
            .WithMany(s => s.ApiEvents)
            .HasForeignKey(e => e.SessionId);

        builder.HasIndex(e => new { e.SessionId, e.CreatedAt })
            .HasDatabaseName("IX_api_events_session");
        builder.HasIndex(e => new { e.Endpoint, e.CreatedAt })
            .HasDatabaseName("IX_api_events_endpoint");
    }
}
```

---

## Registration

In `Program.cs`:

```csharp
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Analytics")));
```

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Analytics": "Server=.;Database=YourDb;Trusted_Connection=True;"
  }
}
```

---

## Migrations

```bash
dotnet ef migrations add InitialAnalyticsSchema --context AnalyticsDbContext
dotnet ef database update --context AnalyticsDbContext
```

---

## Session correlation logic

The middleware must resolve or create a session on every request using this logic:

1. Hash the incoming IP address with SHA-256
2. Query `analytics.sessions` for an open session matching `ip_hash` where `session_end IS NULL` and `session_start >= SYSUTCDATETIME() - 30 minutes`
3. If found → use that `session_id`, update `session_end = now` and increment `event_count`
4. If not found → insert a new `Session` row, use the new `session_id`
5. Determine request type:
   - If the request is a page load (e.g. `Accept: text/html` or a configured route pattern) → insert a `PageEvent`
   - Otherwise → insert an `ApiEvent`
6. All writes must be fire-and-forget (`Task.Run` or a background channel) — never `await` inside the middleware pipeline

---

## Out of scope

- No client-side JavaScript tracking
- No cookies of any kind
- No third-party analytics platforms
- No browser fingerprinting beyond IP + user agent
- No real-time dashboard (queries against the tables directly are sufficient for now)