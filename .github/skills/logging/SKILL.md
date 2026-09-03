---
name: logging
description: "Detalles técnicos del sistema de logs personalizado: LogEntry, LogCentralClient, OfflineLogQueue y middlewares."
---

# Skill: Sistema de Logging en Olimpia

Detalles profundos del sistema de registro distribuido sin dependencias externas.

---

## LogEntry — Estructura del Registro

```csharp
public sealed class LogEntry
{
    public DateTimeOffset Timestamp  { get; init; } = DateTimeOffset.UtcNow;
    public LogType        LogType    { get; set; }
    public string         Level      { get; init; } = string.Empty;
    public string         Category   { get; init; } = string.Empty;
    public string         Message    { get; init; } = string.Empty;
    public string?        TraceId    { get; init; }
    public string?        UserId     { get; set; }
    public string?        RequestId  { get; set; }
    public int?           StatusCode { get; set; }
    public long?          DurationMs { get; set; }
    public string?        Exception  { get; init; }
    public IDictionary<string, object?> Properties { get; init; } = new Dictionary<string, object?>();
}
```

---

## Flujo Automático (LogCentral)

1. **Determinación**: `CustomLogger` determina el `LogType` (Auditoria | Error | Eventos | Request) según el contexto y nivel.
2. **Envío**: Si `LogCentral.Enabled=true`, se envía vía `ILogCentralClient` con reintentos automáticos.
3. **Failover**: Si falla el envío o está deshabilitado, se encola localmente en `OfflineLogQueue` (JSON lines con rotación diaria).

---

## Middlewares de Captura

| Middleware | Rol | Datos Capturados |
|-----------|-----|------------------|
| `AuditMiddleware` | Usuario | `UserId`, `TraceId`, inicio de cronómetro. |
| `RequestLoggingMiddleware` | Métricas | `StatusCode`, `DurationMs`, método HTTP y path. |
| `ExceptionMiddleware` | Errores | Excepciones no controladas -> `LogType.Error`. |

---

## Reglas de Implementación

- **Thread-safety**: El sistema debe manejar accesos concurrentes al sistema de archivos local (uso de `Lock` en C# 13).
- **Graceful Degradation**: Si el servicio central de logs falla, la aplicación NO debe interrumpirse; debe persistir en disco.
- **Serialización**: Usar exclusivamente `System.Text.Json`.