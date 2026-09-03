---
name: 'API Middleware'
description: 'Reglas para Middlewares.'
applyTo: 'src/**/Middleware/**/*.cs'
---
# Middleware
- Mantener responsabilidades únicas:
  - `ExceptionMiddleware`: Captura errores globales.
  - `AuditMiddleware`: Registra datos del Request/User.
  - `RequestLoggingMiddleware`: Registra duración del Request.
- **Orden de ejecución** importa en `Program.cs`.