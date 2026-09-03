---
name: 'API Controllers'
description: 'Reglas para Controladores API.'
applyTo: 'src/**/Controllers/**/*.cs'
---
# Controladores
- Heredar de `ApiController` (base) si existe, o usar `[ApiController]`.
- **Despachar, NO implementar**: Los endpoints despachan via `IMediator`. Cero lógica de negocio.
  - **Commands** (ICommand): `await _mediator.SendAsync(command);`
  - **Queries** (IQuery): `await _mediator.SendQueryAsync(query);`
- **Excepciones**: Manejar `InvalidOperationException`, `KeyNotFoundException` y `ArgumentException` con bloques try/catch y retornar status 400, 404 o 409.
- No inyectar repositorios aquí, solo `IMediator`.
- **Endpoints paginados**: Declarar `[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, [FromQuery] string? sort = null` en la firma. Para filtros: `QueryStringFilterParser.ParseFilters(HttpContext.Request.Query)`. Para sort: `QueryStringFilterParser.ParseSortFields(sort)`. Retornar `Ok(PagedEnvelope<TDto>.FromPagedResult(result))`. Anotar con `[PaginatedEndpoint(...)]` para documentar filtros dinámicos en Swagger.
- **Endpoints no paginados** (detalle por ID, escritura): retornar el payload directo, sin envelope.
- **Visibilidad en Swagger/OpenAPI**: TODOS los parámetros de query string que un endpoint acepta DEBEN declararse con `[FromQuery]` en la firma del método. Excepción: filtros dinámicos con sintaxis de corchetes (`campo[operador]=valor`) que requieren parsing manual del `IQueryCollection`.

## Versionado
- Los controllers concretos viven en `Controllers/V{N}/` (ej: `Controllers/V1/`, `Controllers/V2/`).
- Cada versión es una **clase independiente**: `public sealed class {Feature}Controller : ApiController`.
- Namespace alineado con la carpeta: `Olimpia.Api.Controllers.V{N}` (ej: `Olimpia.Api.Controllers.V1`).
- Cada versión tiene su propia herencia de `ApiController`, constructor e inyección de `IMediator`.
- Cada clase lleva `[ApiVersion("N.0")]` (con `using Asp.Versioning;`) a nivel de clase (major.minor).
- Cada método de endpoint lleva `[MapToApiVersion("N.0")]` inmediatamente antes del atributo HTTP.
- Métodos en versiones nuevas pueden tener el mismo nombre que en versiones anteriores (cada versión es una clase aislada).
- **El versionado es responsabilidad exclusiva de la capa API.** Application, Domain e Infrastructure NO se versionan.
