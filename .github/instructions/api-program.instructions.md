---
name: 'Program.cs'
description: 'Reglas de inicio (Program.cs).'
applyTo: 'src/**/Program.cs'
---
# Program.cs
- Orden de registro estricto: Logging → Controllers → **API Versioning** (`AddApiVersioningConfiguration()`) → Swagger (`AddSwaggerConfiguration(configuration)`) → Autenticación → Authorization → Application → Infrastructure.
- **`AddEndpointsApiExplorer()`** es necesario para que los endpoints de Minimal API (ej: health check) aparezcan en Swagger. `AddApiExplorer()` solo cubre controllers versionados.
- En Development, usar `app.UseSwaggerConfiguration()` para habilitar Swagger UI multi-versión.
- Orden de middleware: Exception → RequestLogging → Audit → Autenticación → Autorización → Controladores. (El versionado no agrega middleware al pipeline.)
