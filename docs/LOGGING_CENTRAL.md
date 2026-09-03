# 📊 Logging Centralizado — OlimpiaIT.Logging (Serilog)

Documentación del sistema de logging corporativo basado en **Serilog** con sink de base de datos.

> **Paquetes:** `OlimpiaIT.Logging.Entities` · `OlimpiaIT.Logging.Serilog.Database`
> **Feed NuGet:** `PaquetesMiSISECTransversal`
> **Guía de referencia:** `D:\SS\SS_ProyectosPaquetesNuget\OlimpiaIT.Serilog\docs\IMPLEMENTATION_GUIDE.md`

---

## Tabla de contenidos

1. [Arquitectura del sistema](#1-arquitectura-del-sistema)
2. [Sink configurado: Solo Base de Datos](#2-sink-configurado-solo-base-de-datos)
3. [Tipos de log y métodos de extensión](#3-tipos-de-log-y-métodos-de-extensión)
4. [Configuración por entorno](#4-configuración-por-entorno)
5. [Esquema de base de datos](#5-esquema-de-base-de-datos)
6. [Uso en clases de aplicación](#6-uso-en-clases-de-aplicación)
7. [Bootstrap en Program.cs](#7-bootstrap-en-programcs)
8. [Gestión segura de secretos](#8-gestión-segura-de-secretos)
9. [Migración desde CustomLogger](#9-migración-desde-customlogger)

---

## 1. Arquitectura del sistema

```
┌────────────────────────────────────────────────────┐
│              Aplicación (Olimpia API)               │
│  ILogger<T> → extension methods → EventId tipado  │
└────────────────┬───────────────────────────────────┘
                 │  Serilog Pipeline
         ┌───────▼────────┐
         │  Serilog Core  │ ← Enrichers: HttpContext, Machine, ThreadId
         └───────┬────────┘
                 │
        ┌────────┴─────────────────┐
        │                          │
        ▼                          ▼
┌───────────────┐         ┌─────────────────────┐
│   Console     │         │   SQL Server / PG   │
│   (Serilog)   │         │   (DB Sink)         │
└───────────────┘         │  AuditLogs          │
        │                 │  ErrorLogs          │
        ▼                 │  EventLogs          │
┌───────────────┐         │  RequestLogs        │
│   File        │         └─────────────────────┘
│   logs/*.log  │
└───────────────┘
```

**Campo `Component` automático:** El sink construye `"{ApplicationName} - {ClassName}"` a partir de `appsettings.json:LogCentralDatabase:ApplicationName` y el `SourceContext` de Serilog.

---

## 2. Sink configurado: Solo Base de Datos

Este proyecto usa el sink **Solo Base de Datos** (`UseSerilogWithDatabaseOnly`). No se envía nada a LogCentral vía HTTP. Los logs se persisten en SQL Server (o PostgreSQL) con tablas espejo de los 4 tipos de log.

**¿Cuándo habilitar también HTTP?** Si el proyecto necesita enviar logs a la Central de Logs vía HTTP, cambiar a `UseSerilogWithDatabase` y agregar `AddLogCentral` + la sección `LogCentralService` en `appsettings.json`. Ver guía de referencia §5.2.

---

## 3. Tipos de log y métodos de extensión

| Método de extensión | EventId | Nivel Serilog | Tabla en DB |
|---|---|---|---|
| `logger.LogAudit(action, param, before, after)` | `1 / "Audit"` | Information | `AuditLogs` |
| `logger.LogError(message, exception, severity)` | `2 / "Error"` | Error | `ErrorLogs` |
| `logger.LogEvent(detail)` | `3 / "Event"` | Information | `EventLogs` |
| `logger.LogRequest(endpoint, method, statusCode, durationMs)` | `4 / "Request"` | Information | `RequestLogs` |
| `logger.LogInfo(message)` | ninguno | Information | `EventLogs` con prefijo `[INFO]` |
| `logger.LogError(exception, "...")` (estándar .NET) | ninguno | Error | `ErrorLogs` |
| Cualquier `LogInformation` sin EventId | ninguno | Information | `EventLogs` con `[INFO]` |

### Namespaces requeridos

```csharp
using Microsoft.Extensions.Logging;
using OlimpiaIT.Logging.Serilog.Extensions;    // LogAudit, LogEvent, LogRequest, LogError
using OlimpiaIT.Logging.Entities.Requests;     // CreateAuditRequest, CreateErrorRequest, etc. (opcional)
```

---

## 4. Configuración por entorno

### `appsettings.json` (base, sin secretos)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/olimpia-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },
  "LogCentralDatabase": {
    "Provider": "SqlServer",
    "MinimumLevel": "Warning",
    "Schema": "dbo",
    "TablePrefix": "",
    "ApplicationName": "BaseApi"
  }
}
```

> **`ConnectionString`** no se pone en el archivo base. Se provee por User Secrets (dev) o variables de entorno (prod/CI). Ver §8.

### `appsettings.Development.json`

```json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Debug" }
  },
  "LogCentralDatabase": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=localhost;Database=OlimpiaLogs_Dev;Integrated Security=True;TrustServerCertificate=True",
    "MinimumLevel": "Debug",
    "Schema": "dbo",
    "TablePrefix": ""
  }
}
```

### `appsettings.Production.json`

```json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information" }
  },
  "LogCentralDatabase": {
    "MinimumLevel": "Warning"
  }
}
```

Los secretos de producción (`ConnectionString`) se inyectan vía variables de entorno:

```bash
LogCentralDatabase__ConnectionString=Server=prod-sql;Database=Logs;User Id=writer;Password=<secret>
```

---

## 5. Esquema de base de datos

Antes del primer despliegue, ejecutar el script DDL del paquete:

```
packages/OlimpiaIT.Logging.Serilog.Database/Scripts/SqlServer/CreateLogTables.sql
```

Buscar y reemplazar `YourSchema` por `dbo`. Tablas creadas:

| Tabla | Contenido |
|---|---|
| `[dbo].[AuditLogs]` | Action, Parameter, BeforeValue, AfterValue, StatusExecution, UserId, Machine, TraceId, Component |
| `[dbo].[ErrorLogs]` | Severity, Description, ExceptionType, StackTrace, UserId, Machine, TraceId, Component |
| `[dbo].[EventLogs]` | Detail, UserId, Machine, TraceId, Component |
| `[dbo].[RequestLogs]` | Endpoint, Type, Status, DurationMs, Request, Response, UserId, Machine, TraceId, Component |

### Política de retención recomendada

```sql
-- Ejecutar diariamente en horario valle
DELETE FROM [dbo].[AuditLogs]   WHERE [CreatedAt] < DATEADD(DAY, -90,  SYSUTCDATETIME());
DELETE FROM [dbo].[ErrorLogs]   WHERE [CreatedAt] < DATEADD(DAY, -180, SYSUTCDATETIME());
DELETE FROM [dbo].[EventLogs]   WHERE [CreatedAt] < DATEADD(DAY, -30,  SYSUTCDATETIME());
DELETE FROM [dbo].[RequestLogs] WHERE [CreatedAt] < DATEADD(DAY, -30,  SYSUTCDATETIME());
```

---

## 6. Uso en clases de aplicación

### Middlewares

Los middlewares `AuditMiddleware` y `RequestLoggingMiddleware` ya usan los métodos de extensión:

```csharp
using OlimpiaIT.Logging.Serilog.Extensions;

// AuditMiddleware
_logger.LogAudit(
    action: "POST /api/products",
    parameter: "IP: 192.168.1.1",
    beforeValue: requestInfo,
    afterValue: responseInfo);

// RequestLoggingMiddleware
logger.LogRequest(
    endpoint: "POST /api/products",
    method: "POST",
    statusCode: 201,
    durationMs: 245);
```

### Handlers

```csharp
using OlimpiaIT.Logging.Serilog.Extensions;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly ILogger<CreateProductHandler> _logger;
    // ...

    public async Task<int> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var id = await _repository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            _logger.LogAudit(
                action: "CreateProduct",
                parameter: "Name",
                beforeValue: null,
                afterValue: $"Id={id}, Name={command.Name}");

            return id;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError("Error al crear producto", ex);
            throw;
        }
    }
}
```

---

## 7. Bootstrap en Program.cs

```csharp
using OlimpiaIT.Logging.Serilog.Database;

var builder = WebApplication.CreateBuilder(args);

// IHttpContextAccessor requerido por HttpContextEnricher
builder.Services.AddHttpContextAccessor();

// Registrar IDbProvider y opciones del sink de BD
builder.Services.AddLogCentralDatabase(builder.Configuration);

// Configurar Serilog: Console + File + DB (sin HTTP a LogCentral)
builder.Host.UseSerilogWithDatabaseOnly(builder.Configuration);
```

---

## 8. Gestión segura de secretos

`ConnectionString` **NUNCA** debe estar en texto plano en el repositorio (salvo conexiones Windows Integrated Security en dev).

### Desarrollo local (User Secrets)

```bash
dotnet user-secrets set "LogCentralDatabase:ConnectionString" "Server=localhost;Database=OlimpiaLogs_Dev;User Id=dev;Password=mi-password"
```

### CI/CD / Docker (variables de entorno)

```bash
# __ (doble guión bajo) como separador jerárquico
LogCentralDatabase__ConnectionString=Server=prod-sql;Database=Logs;User Id=writer;Password=<secret>
```

### Azure Key Vault (producción)

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://mi-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
// LogCentralDatabase--ConnectionString → LogCentralDatabase:ConnectionString
```

---

## 9. Migración desde CustomLogger

Este proyecto migró desde `CustomLogger` / `CustomLoggerProvider` a `OlimpiaIT.Logging.Serilog`.

**Lo que cambió:**
- Eliminados proyectos `Olimpia.Infrastructure.Logging` y `Olimpia.Infrastructure.Logging.Entities`.
- `Program.cs`: `AddLoggingInfrastructure` + `AddCustomLogger` → `AddLogCentralDatabase` + `UseSerilogWithDatabaseOnly`.
- `appsettings.json`: sección `Logging.CustomLogger` → `Serilog` + `LogCentralDatabase`.
- Namespaces en middlewares: `Olimpia.Infrastructure.Logging` → `OlimpiaIT.Logging.Serilog.Extensions`.

**Lo que NO cambió:**
- La interfaz `ILogger<T>` y `ILogger` — sin cambios en el código de aplicación.
- Los métodos de extensión `LogAudit`, `LogError`, `LogEvent`, `LogRequest` tienen las **mismas firmas**.
- Los EventId son los mismos (1=Audit, 2=Error, 3=Event, 4=Request).

---

## Referencias

- [`docs/PATTERNS.md §7`](PATTERNS.md) — Convenciones C# (A1–A18).
- [`.github/copilot-instructions.md`](../.github/copilot-instructions.md) — Stack y arquitectura global.
- Guía de implementación completa: `D:\SS\SS_ProyectosPaquetesNuget\OlimpiaIT.Serilog\docs\IMPLEMENTATION_GUIDE.md`
