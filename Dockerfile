
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY HDA.sln .
COPY src/HDA.Domain/HDA.Domain.csproj src/HDA.Domain/
COPY src/HDA.Infrastructure/HDA.Infrastructure.csproj src/HDA.Infrastructure/
COPY src/HDA.Web/HDA.Web.csproj src/HDA.Web/


RUN dotnet restore HDA.sln


COPY . .


WORKDIR /src/src/HDA.Web
RUN dotnet publish HDA.Web.csproj -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app


RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*


COPY --from=build /app/publish .


RUN mkdir -p wwwroot/uploads/avatars wwwroot/uploads/teams wwwroot/uploads/images

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "HDA.Web.dll"]
