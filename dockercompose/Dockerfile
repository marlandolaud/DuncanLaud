# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Install Node.js 20 — required for React build during dotnet publish
RUN apt-get update && apt-get install -y ca-certificates curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy solution and all project files for restore
COPY DuncanLaud.WebUI/DuncanLaud.WebUI.csproj DuncanLaud.WebUI/
COPY DuncanLaud.Domain/DuncanLaud.Domain.csproj DuncanLaud.Domain/
COPY DuncanLaud.Infrastructure/DuncanLaud.Infrastructure.csproj DuncanLaud.Infrastructure/
COPY DuncanLaud.DTOs/DuncanLaud.DTOs.csproj DuncanLaud.DTOs/

RUN dotnet restore DuncanLaud.WebUI/DuncanLaud.WebUI.csproj

# Copy all source code
COPY DuncanLaud.WebUI/ DuncanLaud.WebUI/
COPY DuncanLaud.Domain/ DuncanLaud.Domain/
COPY DuncanLaud.Infrastructure/ DuncanLaud.Infrastructure/
COPY DuncanLaud.DTOs/ DuncanLaud.DTOs/
COPY duncanlaud-react/ duncanlaud-react/

# Install React dependencies and build
RUN cd duncanlaud-react && npm ci && npm run build

# Publish .NET app
RUN dotnet publish DuncanLaud.WebUI/DuncanLaud.WebUI.csproj \
    --configuration Release \
    --output /app/publish

# Copy React build output into publish wwwroot
RUN cp -r duncanlaud-react/dist/* /app/publish/wwwroot/ 2>/dev/null || \
    mkdir -p /app/publish/wwwroot && cp -r duncanlaud-react/dist/* /app/publish/wwwroot/

# ── Stage 2: Run ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DuncanLaud.WebUI.dll"]
