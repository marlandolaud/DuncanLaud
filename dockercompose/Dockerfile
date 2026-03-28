# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Install Node.js 20 — required for React build during dotnet publish
RUN apt-get update && apt-get install -y ca-certificates curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy solution and all project files for restore
COPY src/DuncanLaud.WebUI/DuncanLaud.WebUI.csproj src/DuncanLaud.WebUI/
COPY src/DuncanLaud.Domain/DuncanLaud.Domain.csproj src/DuncanLaud.Domain/
COPY src/DuncanLaud.Infrastructure/DuncanLaud.Infrastructure.csproj src/DuncanLaud.Infrastructure/
COPY src/DuncanLaud.DTOs/DuncanLaud.DTOs.csproj src/DuncanLaud.DTOs/

RUN dotnet restore src/DuncanLaud.WebUI/DuncanLaud.WebUI.csproj

# Copy all source code
COPY src/DuncanLaud.WebUI/ src/DuncanLaud.WebUI/
COPY src/DuncanLaud.Domain/ src/DuncanLaud.Domain/
COPY src/DuncanLaud.Infrastructure/ src/DuncanLaud.Infrastructure/
COPY src/DuncanLaud.DTOs/ src/DuncanLaud.DTOs/
COPY src/duncanlaud-react/ src/duncanlaud-react/

# Install React dependencies and build
RUN cd src/duncanlaud-react && npm ci && npm run build

# Publish .NET app
RUN dotnet publish src/DuncanLaud.WebUI/DuncanLaud.WebUI.csproj \
    --configuration Release \
    --output /app/publish

# Copy React build output into publish wwwroot
RUN cp -r src/duncanlaud-react/dist/* /app/publish/wwwroot/ 2>/dev/null || \
    mkdir -p /app/publish/wwwroot && cp -r src/duncanlaud-react/dist/* /app/publish/wwwroot/

# ── Stage 2: Run ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DuncanLaud.WebUI.dll"]
