# ⚙️ Configuración - Variables de Entorno y Secretos

Documentación completa de configuración para diferentes entornos (Development, Staging, Production).

---

## Jerarquía de Configuración

.NET Lee configuración en este orden (última gana):

```
1. appsettings.json
2. appsettings.{ASPNETCORE_ENVIRONMENT}.json
3. Variables de entorno
4. User Secrets (solo desarrollo)
5. Program.cs (override programático)
```

---

## 1. appsettings.json (Base)

```json
{
  "Jwt": {
    "Authority": "https://identity.company.com",
    "Audience": "olimpia-template",
    "RequireHttpsMetadata": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OlimpiaPrefixDb;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    },
    "CustomLogger": {
      "MinimumLevel": "Information",
      "Path": "logs"
    },
    "LogCentral": {
      "Enabled": false,
      "BaseUrl": "https://logcentral.company.com",
      "Timeout": 10000,
      "RetryAttempts": 3,
      "FailoverPath": "logs/offline"
    }
  },
  "RedisCache": {
    "Enabled": false,
    "ConnectionString": "localhost:6379",
    "InstanceName": "OlimpiaPrefix_",
    "DefaultExpirationMinutes": 60
  },
  "HttpClient": {
    "RetryEnabled": true,
    "MaxRetryAttempts": 3,
    "InitialDelayMs": 200
  },
  "ExternalApis": {
    "CatalogoService": {
      "BaseUrl": "https://catalogo.internal/"
    },
    "NotificacionesService": {
      "BaseUrl": "https://notificaciones.internal/"
    }
  }
}
```

---

## 2. appsettings.Development.json

```json
{
  "Jwt": {
    "Authority": "http://localhost:5001",
    "Audience": "olimpia-template",
    "RequireHttpsMetadata": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OlimpiaPrefixDb_Dev;User Id=sa;Password=Pass@123!;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "Olimpia": "Debug"
    },
    "CustomLogger": {
      "MinimumLevel": "Debug",
      "Path": "logs"
    },
    "LogCentral": {
      "Enabled": false
    }
  },
  "RedisCache": {
    "Enabled": false
  },
  "HttpClient": {
    "RetryEnabled": true,
    "MaxRetryAttempts": 2,
    "InitialDelayMs": 100
  }
}
```

---

## 3. appsettings.Production.json

```json
{
  "Jwt": {
    "Authority": "https://identity.production.com",
    "Audience": "olimpia-template-prod",
    "RequireHttpsMetadata": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Error",
      "Microsoft.AspNetCore": "Error"
    },
    "CustomLogger": {
      "MinimumLevel": "Information",
      "Path": "/var/log/olimpia-template"
    },
    "LogCentral": {
      "Enabled": true,
      "BaseUrl": "https://logcentral.production.com",
      "Timeout": 30000,
      "RetryAttempts": 5
    }
  },
  "RedisCache": {
    "Enabled": true,
    "ConnectionString": "redis-cluster.production:6379,ssl=true",
    "InstanceName": "OlimpiaPrefix_Prod_",
    "DefaultExpirationMinutes": 120
  },
  "HttpClient": {
    "RetryEnabled": true,
    "MaxRetryAttempts": 5,
    "InitialDelayMs": 500
  }
}
```

---

## 4. Variables de Entorno

### Convención: Separador `__` (doble guion bajo)

Las variables de entorno se mapean a `appsettings.json` usando `__` como separador de niveles.

```bash
# ── Host ───────────────────────────────────────────────────────────────────
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=8080

# ── Base de datos ──────────────────────────────────────────────────────────
ConnectionStrings__DefaultConnection=Server=proddb;Database=OlimpiaPrefixDb;User Id=sa;Password=SECRETO;

# ── JWT multi-proveedor (índice 0 = proveedor principal) ───────────────────
Jwt__Providers__0__Name=OpenIddict
Jwt__Providers__0__Type=Oidc
Jwt__Providers__0__Enabled=true
Jwt__Providers__0__Authority=https://identity.production.com
Jwt__Providers__0__Audience=olimpia-template-prod
Jwt__Providers__0__RequireHttpsMetadata=true

# ── Serilog ────────────────────────────────────────────────────────────────
Serilog__MinimumLevel__Default=Information

# ── LogCentral Database (logs estructurados en BD) ─────────────────────────
LogCentralDatabase__Provider=SqlServer
LogCentralDatabase__MinimumLevel=Warning
LogCentralDatabase__Schema=dbo
LogCentralDatabase__ApplicationName=BaseApi

# ── Reintentos en repositorios ─────────────────────────────────────────────
Repository__RetryEnabled=true
Repository__MaxRetryAttempts=3
Repository__InitialDelayMs=100

# ── Reintentos en clientes HTTP ────────────────────────────────────────────
HttpClient__RetryEnabled=true
HttpClient__MaxRetryAttempts=5
HttpClient__InitialDelayMs=500

# ── Redis Cache ────────────────────────────────────────────────────────────
RedisCache__Enabled=true
RedisCache__ConnectionString=redis-cluster.production:6379,ssl=true
RedisCache__InstanceName=OlimpiaPrefix_Prod_
RedisCache__DefaultExpirationMinutes=120

# ── APIs externas ──────────────────────────────────────────────────────────
ExternalApis__LogCentralService__BaseUrl=https://logcentral.production.com
ExternalApis__CatalogoService__BaseUrl=https://catalogo.production.com
ExternalApis__NotificacionesService__BaseUrl=https://notificaciones.production.com
```

---

## 5. Program.cs - Lectura de Configuración

```csharp
// Olimpia.Api/Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);
var configuration = builder.Configuration;

// Las variables de entorno se cargan automáticamente por .NET
// La jerarquía se aplica automáticamente

// 1. Autenticación JWT multi-proveedor (Jwt:Providers[])
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
// Valida que al menos un proveedor esté habilitado antes de iniciar
if (!jwtOptions.Providers.Any(p => p.Enabled))
    throw new InvalidOperationException("No hay proveedores JWT habilitados. Revise 'Jwt:Providers' en appsettings.json.");
// Ver src/Olimpia.Api/Program.cs para el setup completo de PolicyScheme + ForwardDefaultSelector.

// 2. Database
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<UnitOfWork>();

// 3. Logging
builder.Logging.ClearProviders();
builder.Logging.AddCustomLogger(configuration);

// 4. Redis Cache (si está habilitado)
var cacheConfig = configuration.GetSection("RedisCache");
if (cacheConfig.GetValue<bool>("Enabled"))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheConfig["ConnectionString"];
        options.InstanceName = cacheConfig["InstanceName"];
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// 5. HTTP Clients
builder.Services.AddInfrastructure(configuration);

// 6. Application
builder.Services.AddApplication();

var app = builder.Build();

app.Use Swagger();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 6. Secretos en Desarrollo (User Secrets)

Para contraseñas, API keys, etc. en **desarrollo**:

```bash
# Inicializar user secrets
dotnet user-secrets init

# Agregar secretos (se guardan en %APPDATA%\Microsoft\UserSecrets)
dotnet user-secrets set "Jwt:Providers:0:Authority" "http://localhost:5001"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=OlimpiaPrefixDb;User Id=sa;Password=SecretPassword123;"

# Listar secretos
dotnet user-secrets list

# Borrar un secreto
dotnet user-secrets remove "Jwt:Providers:0:Authority"
```

**Nota:** User secrets se cargan automáticamente en `IConfiguration`, solo en ambiente `Development`.

---

## 7. .env para Docker

```bash
# .env (o .env.production)
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=8080

ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OlimpiaPrefixDb;User Id=sa;Password=PRODUCTSECRET;TrustServerCertificate=True;

Jwt__Providers__0__Name=OpenIddict
Jwt__Providers__0__Type=Oidc
Jwt__Providers__0__Enabled=true
Jwt__Providers__0__Authority=https://identity.production.com
Jwt__Providers__0__Audience=olimpia-template-prod
Jwt__Providers__0__RequireHttpsMetadata=true

Serilog__MinimumLevel__Default=Information

LogCentralDatabase__Provider=SqlServer
LogCentralDatabase__MinimumLevel=Warning
LogCentralDatabase__Schema=dbo
LogCentralDatabase__ApplicationName=BaseApi

Repository__RetryEnabled=true
Repository__MaxRetryAttempts=3
Repository__InitialDelayMs=100

HttpClient__RetryEnabled=true
HttpClient__MaxRetryAttempts=5
HttpClient__InitialDelayMs=500

RedisCache__Enabled=true
RedisCache__ConnectionString=redis-cluster:6379,ssl=true
RedisCache__InstanceName=OlimpiaPrefix_Prod_
RedisCache__DefaultExpirationMinutes=120

ExternalApis__LogCentralService__BaseUrl=https://logcentral.production.com
ExternalApis__CatalogoService__BaseUrl=https://catalogo.production.com
ExternalApis__NotificacionesService__BaseUrl=https://notificaciones.production.com
```

### Docker Compose - Cargar .env

```yaml
version: '3.8'
services:
  api:
    image: olimpia-template
    ports:
      - "8080:8080"
    env_file:
      - .env
    depends_on:
      - sqlserver
      - redis
  
  sqlserver:
    image: mcr.microsoft.com/mssql/server:latest
    environment:
      SA_PASSWORD: PRODUCTSECRET
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
  
  redis:
    image: redis:latest
    ports:
      - "6379:6379"
```

---

## 8. Configuración Segura en Producción

### ❌ **Nunca** en appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db;Password=SecretPassword;"  // ❌ NO!
  }
}
```

### ✅ **Siempre** via Variables de Entorno

```bash
# En el servidor o Azure Key Vault
ConnectionStrings__DefaultConnection=Server=db;Password=$(PROD_DB_PASSWORD);
```

### Azure Key Vault Integration

```csharp
// Program.cs
var keyVaultUrl = new Uri($"https://{keyVaultName}.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
```

### Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: olimpia-template-secrets
type: Opaque
stringData:
  Jwt__Providers__0__Authority: https://identity.k8s.company.com
  Jwt__Providers__0__Type: Oidc
  Jwt__Providers__0__Enabled: "true"
  Jwt__Providers__0__Audience: olimpia-template-k8s
  ConnectionStrings__DefaultConnection: Server=sql-svc;Database=OlimpiaPrefixDb;...
  Logging__LogCentral__BaseUrl: https://logcentral.k8s.company.com
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: olimpia-template
spec:
  template:
    spec:
      containers:
      - name: api
        image: olimpia-template:latest
        envFrom:
        - secretRef:
            name: olimpia-template-secrets
```

---

## 9. Validación de Configuración

```csharp
// Program.cs
var app = builder.Build();

// Validar que configuración es completa
var jwtOpts = app.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
if (!jwtOpts.Providers.Any(p => p.Enabled))
{
    throw new InvalidOperationException("No hay proveedores JWT habilitados en la configuración.");
}

var connString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connString))
{
    throw new InvalidOperationException("ConnectionString no configurada");
}

app.Logger.LogInformation("Configuración validada correctamente");
app.Run();
```

---

## 10. Tabla de Referencia Rápida

| Clave | Tipo | Default (dev) | Producción |
|-------|------|---------------|------------|
| `ConnectionStrings:DefaultConnection` | string | (localdb) | SQL Server en red |
| `Jwt:Providers:0:Name` | string | OpenIddict | OpenIddict |
| `Jwt:Providers:0:Type` | string | Oidc | Oidc |
| `Jwt:Providers:0:Enabled` | bool | true | true |
| `Jwt:Providers:0:Authority` | string | http://localhost:5001 | https://identity.company.com |
| `Jwt:Providers:0:Audience` | string | olimpia-template | olimpia-template-prod |
| `Jwt:Providers:0:RequireHttpsMetadata` | bool | false | true |
| `Serilog:MinimumLevel:Default` | string | Information | Information |
| `LogCentralDatabase:Provider` | string | SqlServer | SqlServer |
| `LogCentralDatabase:MinimumLevel` | string | Warning | Warning |
| `LogCentralDatabase:ApplicationName` | string | BaseApi | BaseApi |
| `Repository:RetryEnabled` | bool | true | true |
| `Repository:MaxRetryAttempts` | int | 3 | 3 |
| `Repository:InitialDelayMs` | int | 100 | 100 |
| `HttpClient:RetryEnabled` | bool | true | true |
| `HttpClient:MaxRetryAttempts` | int | 3 | 5 |
| `HttpClient:InitialDelayMs` | int | 200 | 500 |
| `RedisCache:Enabled` | bool | false | true |
| `RedisCache:ConnectionString` | string | localhost:6379 | redis-cluster:6379 |
| `RedisCache:InstanceName` | string | OlimpiaPrefix_ | OlimpiaPrefix_Prod_ |
| `RedisCache:DefaultExpirationMinutes` | int | 60 | 120 |
| `ExternalApis:LogCentralService:BaseUrl` | string | https://logcentral.internal/ | https://logcentral.production.com |
| `ExternalApis:CatalogoService:BaseUrl` | string | https://catalogo.internal/ | https://catalogo.production.com |
| `ExternalApis:NotificacionesService:BaseUrl` | string | https://notificaciones.internal/ | https://notificaciones.production.com |

---

## Próximos Pasos

- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Docker y Kubernetes
- **[LOGGING_CENTRAL.md](LOGGING_CENTRAL.md)** - LogCentral
