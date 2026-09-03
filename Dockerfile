# Imagen base con ASP.NET Core 10 runtime.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Olimpia.Api/Olimpia.Api.csproj", "src/Olimpia.Api/"]
COPY ["src/Olimpia.Application/Olimpia.Application.csproj", "src/Olimpia.Application/"]
COPY ["src/Olimpia.Domain/Olimpia.Domain.csproj", "src/Olimpia.Domain/"]
COPY ["src/Olimpia.Infrastructure/Olimpia.Infrastructure.csproj", "src/Olimpia.Infrastructure/"]
COPY ["nuget.config", "."]
RUN dotnet restore "src/Olimpia.Api/Olimpia.Api.csproj"
COPY . .
WORKDIR "/src/src/Olimpia.Api"
RUN dotnet build "Olimpia.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Olimpia.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Inicio código generado por GitHub Copilot
# Variables de entorno configurables en tiempo de ejecución.
# Todas pueden sobreescribirse con -e al hacer docker run o en docker-compose.
# Usar __ como separador jerárquico (ej. ConnectionStrings__DefaultConnection).

# Configuración del host ASP.NET Core.
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

# Cadena de conexión a SQL Server (obligatoria; proveer en tiempo de ejecución).
ENV ConnectionStrings__DefaultConnection=""

# JWT multi-proveedor: índice 0 corresponde al proveedor principal (OpenIddict/OIDC).
ENV Jwt__Providers__0__Name=OpenIddict \
    Jwt__Providers__0__Type=Oidc \
    Jwt__Providers__0__Enabled=true \
    Jwt__Providers__0__Authority="" \
    Jwt__Providers__0__Audience=olimpia-template \
    Jwt__Providers__0__RequireHttpsMetadata=true

# Serilog: nivel mínimo de logs.
ENV Serilog__MinimumLevel__Default=Information

# LogCentral Database: escritura de logs estructurados en base de datos.
ENV LogCentralDatabase__Provider=SqlServer \
    LogCentralDatabase__MinimumLevel=Warning \
    LogCentralDatabase__Schema=dbo \
    LogCentralDatabase__ApplicationName=BaseApi

# Reintentos en repositorios de datos.
ENV Repository__RetryEnabled=true \
    Repository__MaxRetryAttempts=3 \
    Repository__InitialDelayMs=100

# Reintentos en clientes HTTP.
ENV HttpClient__RetryEnabled=true \
    HttpClient__MaxRetryAttempts=3 \
    HttpClient__InitialDelayMs=200

# Redis Cache: deshabilitado por defecto; habilitar en producción.
ENV RedisCache__Enabled=false \
    RedisCache__ConnectionString="" \
    RedisCache__InstanceName=OlimpiaPrefix_ \
    RedisCache__DefaultExpirationMinutes=60

# APIs externas: proveer URLs según entorno de despliegue.
ENV ExternalApis__LogCentralService__BaseUrl="" \
    ExternalApis__CatalogoService__BaseUrl="" \
    ExternalApis__NotificacionesService__BaseUrl=""
# Fin código generado por GitHub Copilot

ENTRYPOINT ["dotnet", "Olimpia.Api.dll"]
