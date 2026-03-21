# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Install Node.js 20 — required by the csproj BuildReactApp MSBuild target
# which runs npm install + npm run build during dotnet publish
RUN apt-get update && apt-get install -y ca-certificates curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy both projects — csproj SpaRoot points to ../duncanlaud-react/ relative to DuncanLaud.WebUI/
COPY duncanlaud-react/ duncanlaud-react/
COPY DuncanLaud.WebUI/ DuncanLaud.WebUI/

# Publish — triggers BuildReactApp (npm install + npm run build) and
# PublishReactApp (copies dist/ into wwwroot/) via MSBuild targets
RUN dotnet publish DuncanLaud.WebUI/DuncanLaud.WebUI.csproj \
    --configuration Release \
    --output /app/publish

# ── Stage 2: Run ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DuncanLaud.WebUI.dll"]
