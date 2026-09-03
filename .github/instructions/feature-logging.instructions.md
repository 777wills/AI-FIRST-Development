---
name: 'Logging Centralizado'
description: 'Reglas de logs.'
applyTo: 'src/**/Logging/**/*.cs'
---
# Logging Personalizado
- LogType es automático (Eventos, Request, Auditoria, Error).
- Extraer `UserId` y `RequestId` del HttpContext.
- Integración con LogCentral usa `ILogCentralClient` con reintentos y encolado offline (`OfflineLogQueue`).