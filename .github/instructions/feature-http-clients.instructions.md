---
name: 'HTTP Clients Externa'
description: 'Consumo de APIs Externas.'
applyTo: 'src/**/Http/**/*.cs,src/**/Contracts/IExternalApiClient.cs'
---
# Clientes HTTP Externos
- Usar `IExternalApiClient` inyectado en la capa Application.
- El token JWT se propaga automáticamente mediante `BearerTokenPropagationHandler`.
- Los reintentos (Polly) se gestionan transparentemente a través de `PollyRetryHandler`.