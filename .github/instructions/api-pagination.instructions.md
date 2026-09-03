---
name: 'Paginación API'
description: 'Reglas de paginación, filtrado, ordenamiento y envelope para endpoints de listado.'
applyTo: 'src/**/Queries/**/*Query.cs,src/**/Queries/**/*Handler.cs,src/**/Queries/**/*Validator.cs,src/**/Controllers/**/*.cs'
---
# Paginación, Filtrado y Ordenamiento

## Queries Paginadas

- Heredar de `PagedQuery` (abstract record en `Olimpia.Application.Common.Pagination`).
- Implementar `IQuery<PagedResult<TDto>>` de `Cortex.Mediator.Queries`.
- `PagedQuery` provee: `PageNumber` (default 1), `PageSize` (default 25), `Filters` (nullable), `SortFields` (nullable).

```csharp
public sealed record GetAllXxxQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null)
    : PagedQuery(PageNumber, PageSize, Filters, SortFields), IQuery<PagedResult<XxxDto>>;
```

## Handlers Paginados

- Implementar `IQueryHandler<TQuery, PagedResult<TDto>>`.
- Llamar `_repository.GetPagedAsync(query.PageNumber, query.PageSize, query.Filters, sortFields)`.
- Mapear entidades a DTOs manualmente (no AutoMapper).
- Retornar `PagedResult<TDto>.Create(dtos, pageNumber, pageSize, totalCount)`.
- Sin `IUnitOfWork` (son lecturas).
- **Sort por defecto:** Si la query tiene un sort por defecto definido por negocio, el handler lo aplica cuando `SortFields` es null o vacío. Ejemplo: `var sortFields = query.SortFields is { Count: > 0 } ? query.SortFields : DefaultSort;`

## Validators de Queries Paginadas

Cada validator define su **whitelist** de campos filtrables y ordenables.

```csharp
private static readonly Dictionary<string, IReadOnlyList<FilterOperator>> AllowedFilters =
    new(StringComparer.OrdinalIgnoreCase) { ... };
private static readonly HashSet<string> AllowedSortFields =
    new([...], StringComparer.OrdinalIgnoreCase);
```

Reglas obligatorias:
- `PageNumber >= 1`.
- `PageSize` entre 1 y 100.
- Cada filtro: campo en whitelist Y operador permitido para ese campo.
- Cada campo de ordenamiento en whitelist.
- Mensajes de error en español.

## Envelope de Respuesta

Endpoints paginados retornan `PagedEnvelope<T>` con formato `{ data, meta }`.
- Construir: `PagedEnvelope<TDto>.FromPagedResult(result)`.
- Endpoints no paginados (detalle por ID, mutaciones) NO usan envelope.

## QueryStringFilterParser

- Ubicación: `src/Olimpia.Api/Extensions/QueryStringFilterParser.cs`.
- Convierte query string HTTP a tipos de dominio (`FilterCriteria`, `SortCriteria`).
- **Métodos públicos:**
  - `ParseFilters(IQueryCollection query)` → `List<FilterCriteria>` — extrae filtros con sintaxis `campo[operador]=valor`.
  - `ParseSortFields(string? sort)` → `List<SortCriteria>` — parsea string comma-separated con prefijo `-` para descendente.
- Los parámetros estándar (`pageNumber`, `pageSize`, `sort`) se reciben como `[FromQuery]` en la firma del controller — NO se extraen del parser.
- El parser no valida contra whitelist; eso es responsabilidad del Validator.

## Controller

Los parámetros estándar de paginación (`pageNumber`, `pageSize`, `sort`) DEBEN declararse con `[FromQuery]` en la firma del método para que sean visibles en Swagger/OpenAPI. Los filtros dinámicos (`campo[operador]=valor`) se parsean manualmente del `IQueryCollection`.

```csharp
[HttpGet]
[Authorize(Policy = "xxx.read")]
[PaginatedEndpoint(
    AllowedFilters = "campo1[op],campo2[op]",
    AllowedSortFields = "campo1,campo2",
    DefaultSort = "-createdAt")]
public async Task<IActionResult> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25,
    [FromQuery] string? sort = null)
{
    var filters = QueryStringFilterParser.ParseFilters(HttpContext.Request.Query);
    var sortFields = QueryStringFilterParser.ParseSortFields(sort);
    var query = new GetAllXxxQuery(pageNumber, pageSize, filters, sortFields.Count > 0 ? sortFields : null);
    var result = await _mediator.SendQueryAsync(query);
    return Ok(PagedEnvelope<XxxDto>.FromPagedResult(result));
}
```

## Swagger/OpenAPI

- Los parámetros `pageNumber`, `pageSize` y `sort` son visibles automáticamente en Swagger gracias a `[FromQuery]`.
- Los filtros dinámicos (`campo[operador]=valor`) se documentan mediante el atributo `[PaginatedEndpoint]` y el `PaginatedEndpointOperationFilter` registrado en Swagger.
- El `IOperationFilter` enriquece la descripción del endpoint con la sintaxis de filtros, campos permitidos y ejemplos.
