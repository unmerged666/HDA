# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY HDA.sln .
COPY src/HDA.Domain/HDA.Domain.csproj src/HDA.Domain/
COPY src/HDA.Infrastructure/HDA.Infrastructure.csproj src/HDA.Infrastructure/
COPY src/HDA.Web/HDA.Web.csproj src/HDA.Web/

# Restore NuGet packages
RUN dotnet restore HDA.sln

# Copy all source code
COPY . .

# Build & publish
WORKDIR /src/src/HDA.Web
RUN dotnet publish HDA.Web.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create upload directories
RUN mkdir -p wwwroot/uploads/avatars wwwroot/uploads/teams wwwroot/uploads/images

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "HDA.Web.dll"]
