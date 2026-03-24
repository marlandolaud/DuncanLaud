# N-Layer Architecture Overview

## Projects

```
DuncanLaud.WebUI.sln
├── DuncanLaud.Domain          (Class Library — no external deps)
├── DuncanLaud.Infrastructure  (Class Library — EF Core, AWS SDK, ProfanityFilter)
├── DuncanLaud.DTOs            (Class Library — DataAnnotations only)
└── DuncanLaud.WebUI           (ASP.NET Core Web App — controllers, startup)
```

## Dependency Direction

```
DuncanLaud.WebUI
  ├─→ DuncanLaud.Domain
  ├─→ DuncanLaud.Infrastructure
  └─→ DuncanLaud.DTOs

DuncanLaud.Infrastructure
  ├─→ DuncanLaud.Domain
  └─→ DuncanLaud.DTOs

DuncanLaud.DTOs
  └─→ (nothing — BCL + System.ComponentModel.DataAnnotations only)

DuncanLaud.Domain
  └─→ (nothing — BCL only)
```

No circular dependencies. Domain is the innermost layer; WebUI is the outermost.

## Layer Responsibilities

### DuncanLaud.Domain
Pure business logic with zero external NuGet dependencies.
- `Services/BirthdayCalculator.cs` — computes days until birthday, handles leap years and year-boundary wrap
- `Services/PersonValidator.cs` — name length rules (2-100 chars), birth date range validation
- `ValueObjects/BirthdayResult.cs` — immutable result record
- `Commands/CreatePersonCommand.cs` — input data for creating a person

### DuncanLaud.Infrastructure
Data access, EF Core, external integrations, and interfaces.
- `Entities/` — EF Core entity classes (Group, Person) with fluent configuration in AppDbContext
- `Data/AppDbContext.cs` — SQL Server DbContext with index configuration
- `Data/AppDbContextFactory.cs` — IDesignTimeDbContextFactory for `dotnet ef` CLI
- `Interfaces/` — Service and repository contracts (IGroupService, IPersonService, IGroupRepository, IPersonRepository, IImageStorageService)
- `Repositories/` — EF Core repository implementations
- `Services/GroupService.cs` — get-or-create group logic
- `Services/PersonService.cs` — orchestrates validation, profanity check, and persistence
- `Storage/CloudflareImageService.cs` — Cloudflare Images Direct Creator Upload (production)
- `Storage/MinioImageService.cs` — MinIO S3-compatible pre-signed URL (local development)

### DuncanLaud.DTOs
Shared data transfer objects with validation attributes.
- Request DTOs: `CreateGroupRequest`, `CreatePersonRequest`
- Response DTOs: `GroupResponse`, `PersonResponse`, `BirthdayResponse`, `UploadUrlResponse`
- No logic — plain C# records

### DuncanLaud.WebUI
Thin presentation layer. Controllers map HTTP requests to service calls and DTOs.
- `Controllers/GroupController.cs` — 4 endpoints (POST /api/group, GET /api/group/{id}, POST /api/group/{id}/person, GET /api/group/{id}/birthdays)
- `Controllers/ImageController.cs` — 1 endpoint (GET /api/image/upload-url)
- `Startup.cs` — DI registration, security headers, rate limiting, static file serving

## Security Measures (OWASP Top 10)

| Threat | Implementation |
|---|---|
| A03 Injection | EF Core parameterised queries; no raw SQL string concatenation |
| A01 Broken Access Control | No public group enumeration endpoint; UUID-only access |
| A05 Misconfiguration | Security headers middleware; HSTS in prod; Server header removed |
| A06 Vulnerable Components | Pinned NuGet versions; run `dotnet list package --vulnerable` in CI |
| A07 Auth Failures | IP-based rate limiting via AspNetCoreRateLimit (10 adds/min, 5 creates/min) |
| A08 Integrity Failures | DataAnnotations + ProfanityFilter + image URL domain allowlist |
| A10 SSRF | Pre-signed URL pattern: .NET never proxies image bytes; URL domain allowlisted |

## Running EF Core Migrations

```bash
# From solution root
dotnet tool restore

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project DuncanLaud.Infrastructure \
  --startup-project DuncanLaud.WebUI

# Apply migrations to database
dotnet ef database update \
  --project DuncanLaud.Infrastructure \
  --startup-project DuncanLaud.WebUI
```

## Environment Variables (Production)

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `ImageStorage__Provider` | `Cloudflare` |
| `ImageStorage__CloudflareAccountId` | Cloudflare account ID |
| `ImageStorage__CloudflareApiToken` | CF Images API token (`account:images:write`) |
