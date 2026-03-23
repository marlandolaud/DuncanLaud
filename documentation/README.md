# DuncanLaud — Birthday Groups Feature Documentation

## Index

| Document | Description |
|---|---|
| [erd.md](erd.md) | Entity-Relationship Diagram (Mermaid) |
| [sequences/create-group.md](sequences/create-group.md) | Create a new birthday group |
| [sequences/add-person.md](sequences/add-person.md) | Add a person to a group (with image upload) |
| [sequences/view-birthdays.md](sequences/view-birthdays.md) | View upcoming birthdays |
| [architecture/n-layer-overview.md](architecture/n-layer-overview.md) | N-layer architecture overview and dependency rules |

## Quick Start (Local)

### Prerequisites
- Docker Desktop
- .NET 10 SDK
- Node.js 20+

### 1. Start infrastructure
```bash
cd dockercompose
cp .env.example .env        # fill in passwords
docker compose up db minio  # start SQL Server + MinIO only
```

### 2. Run migrations
```bash
dotnet tool restore
dotnet ef migrations add InitialCreate \
  --project DuncanLaud.Infrastructure \
  --startup-project DuncanLaud.WebUI
dotnet ef database update \
  --project DuncanLaud.Infrastructure \
  --startup-project DuncanLaud.WebUI
```

### 3. Run .NET backend
```bash
cd DuncanLaud.WebUI
dotnet run
```

### 4. Run React dev server
```bash
cd duncanlaud-react
npm install
npm run dev
```

Navigate to `http://localhost:3000` — the Vite proxy forwards `/api/*` to the .NET backend.

## Production Deployment

Set the following GitHub secrets / environment variables:
- `ConnectionStrings__DefaultConnection` — SQL Server connection string
- `ImageStorage__Provider` — `Cloudflare`
- `ImageStorage__CloudflareAccountId` — Your CF Account ID
- `ImageStorage__CloudflareApiToken` — CF Images API token with `account:images:write` scope

The birthday delivery URL format is: `https://imagedelivery.net/{accountId}/{imageId}/public`
