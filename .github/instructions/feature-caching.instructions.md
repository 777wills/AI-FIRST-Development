---
name: 'Caché Distribuido'
description: 'Patrón Cache-Aside con Redis.'
applyTo: 'src/**/Queries/**/*Handler.cs'
---
# Caché en Queries
- Implementar Cache-Aside en Queries:
  1. Intentar obtener de `IDistributedCache`.
  2. Si existe (HIT), retornar.
  3. Si no existe (MISS), buscar en base de datos.
  4. Guardar en caché usando `DistributedCacheEntryOptions` y retornar.