# 📄 Paginación, Filtrado y Ordenamiento

Infraestructura reutilizable para listados paginados en Olimpia. Cualquier endpoint de tipo "GetAll" o "List" debe seguir este estándar.

---

## Visión General

El proyecto utiliza **paginación offset-based** con filtrado por operadores y ordenamiento multi-campo. La infraestructura se distribuye en tres capas:

| Capa | Tipos | Propósito |
|------|-------|-----------|
| **Domain** (`Common/`) | `FilterOperator`, `FilterCriteria`, `SortCriteria` | Tipos puros para expresar filtros y orden — sin dependencias externas |
| **Application** (`Common/`) | `PagedQuery`, `PagedResult<T>`, `PagedEnvelope<T>` | Contratos de paginación y envelope HTTP |
| **Api** (`Extensions/`) | `QueryStringFilterParser` | Parsing de query string → tipos de dominio |

---

## 1. Tipos del Dominio

Los tipos viven en `src/Olimpia.Domain/Common/` porque `IGenericRepository<T>` los referencia directamente.

### FilterOperator (enum)

```csharp
public enum FilterOperator
{
    Eq,        // Igual a
    Neq,       // Diferente de
    Gt,        // Mayor que
    Gte,       // Mayor o igual que
    Lt,        // Menor que
    Lte,       // Menor o igual que
    Contains   // Contiene (LIKE %valor%)
}
```

### FilterCriteria (record)

```csharp
public sealed record FilterCriteria(string Field, FilterOperator Operator, string Value);
```

### SortCriteria (record)

```csharp
public sealed record SortCriteria(string Field, bool Descending);
```

---

## 2. Contratos de Application

### PagedQuery (abstract record)

Base para cualquier query paginada. Ubicación: `src/Olimpia.Application/Common/Pagination/PagedQuery.cs`.

```csharp
public abstract record PagedQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null);
```

### PagedResult\<T\> (sealed record)

Resultado interno del handler. Ubicación: `src/Olimpia.Application/Common/Pagination/PagedResult.cs`.

```csharp
public sealed record PagedResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public static PagedResult<T> Create(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount) => ...
}
```

### PagedEnvelope\<T\> (sealed record)

Envelope estándar para la respuesta HTTP. Ubicación: `src/Olimpia.Application/Common/Responses/PagedEnvelope.cs`.

```csharp
public sealed record PaginationMeta(
    int CurrentPage, int PageSize, int TotalCount, int TotalPages, bool HasNextPage, bool HasPreviousPage);
public sealed record PagedMeta(PaginationMeta Pagination);
public sealed record PagedEnvelope<T>(IEnumerable<T> Data, PagedMeta Meta)
{
    public static PagedEnvelope<T> FromPagedResult(PagedResult<T> result) => ...
}
```

---

## 3. Formato de Respuesta JSON

Todos los endpoints paginados retornan exactamente este formato:

```json
{
  "data": [
    { "id": 1, "name": "Laptop Pro", "price": 1500.00, ... }
  ],
  "meta": {
    "pagination": {
      "currentPage": 1,
      "pageSize": 25,
      "totalCount": 120,
      "totalPages": 5,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  }
}
```

> ⚠️ El envelope aplica **solo a endpoints paginados**. Endpoints de detalle (`GET /{id}`) o de escritura retornan su payload directo.

---

## 4. Query String API

> **Swagger/OpenAPI:** Los parámetros `pageNumber`, `pageSize` y `sort` se declaran con `[FromQuery]` en la firma del endpoint y son visibles en Swagger UI. Los filtros dinámicos (`campo[operador]=valor`) se documentan via `PaginatedEndpointOperationFilter`.

### Paginación

| Parámetro | Default | Ejemplo |
|-----------|---------|---------|
| `pageNumber` | `1` | `?pageNumber=2` |
| `pageSize` | `25` (máx: 100) | `?pageSize=10` |

### Filtrado

Formato: `campo[operador]=valor`. Los filtros se combinan con **AND lógico**.

| Ejemplo | Significado |
|---------|-------------|
| `?name[contains]=Laptop` | Nombre contiene "Laptop" |
| `?price[gte]=100&price[lte]=500` | Precio entre 100 y 500 |
| `?stock[gt]=0` | Stock mayor a 0 |
| `?createdAt[gt]=2026-01-01` | Creados después del 1 de enero de 2026 |

### Ordenamiento

Formato: `sort=campo1,-campo2`. El prefijo `-` indica descendente.

| Ejemplo | Significado |
|---------|-------------|
| `?sort=name` | Nombre ascendente |
| `?sort=-price` | Precio descendente |
| `?sort=name,-price` | Nombre ASC, luego precio DESC |

### Ejemplo Combinado

```
GET /api/v1/products?name[contains]=Laptop&price[gte]=100&sort=name,-price&pageNumber=1&pageSize=10
```

---

## 5. Whitelist por Entidad

Cada entidad define su propia whitelist de campos filtrables y ordenables en su **Validator**.

### Products

**Campos filtrables:**

| Campo | Operadores permitidos |
|-------|----------------------|
| `Name` | `Contains` |
| `Price` | `Eq`, `Gt`, `Gte`, `Lt`, `Lte` |
| `Stock` | `Eq`, `Gt`, `Gte`, `Lt`, `Lte` |
| `CreatedAt` | `Gt`, `Lt` |

**Campos ordenables:** `Name`, `Price`, `Stock`, `CreatedAt`.
**Orden por defecto:** `CreatedAt` descendente.

---

## 6. Implementación en Repositorio

`IGenericRepository<T>` expone `GetPagedAsync` — implementado en `GenericRepository<T>` con SqlKata:

```csharp
Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    IReadOnlyList<FilterCriteria>? filters,
    IReadOnlyList<SortCriteria>? sortFields);
```

- **COUNT** y **DATA** son queries SQL separadas para mejor rendimiento.
- Los filtros se traducen a SqlKata vía `ApplyFilter` (método privado) — los valores se parametrizan automáticamente.
- Para `Contains`, se escapan `%` y `_` antes de aplicar `WhereLike`.

Ver [**DATA_ACCESS.md — Paginación con GetPagedAsync**](DATA_ACCESS.md#paginación-con-getpagedasync) para detalles de implementación.

---

## 7. Cómo Agregar Paginación a un Nuevo Feature

### Paso 1 — Crear la Query

Heredar de `PagedQuery` e implementar `IQuery<PagedResult<TDto>>`:

```csharp
public sealed record GetAllOrdersQuery(
    int PageNumber = 1,
    int PageSize = 25,
    IReadOnlyList<FilterCriteria>? Filters = null,
    IReadOnlyList<SortCriteria>? SortFields = null)
    : PagedQuery(PageNumber, PageSize, Filters, SortFields), IQuery<PagedResult<OrderDto>>;
```

### Paso 2 — Implementar el Handler

```csharp
public sealed class GetAllOrdersHandler : IQueryHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery query, CancellationToken ct)
    {
        var (data, totalCount) = await _orderRepository.GetPagedAsync(
            query.PageNumber, query.PageSize, query.Filters, query.SortFields);

        var dtos = data.Select(o => new OrderDto(o.Id, o.Total, ...));

        return PagedResult<OrderDto>.Create(dtos, query.PageNumber, query.PageSize, totalCount);
    }
}
```

### Paso 3 — Definir Whitelist en el Validator

```csharp
public sealed class GetAllOrdersValidator : AbstractValidator<GetAllOrdersQuery>
{
    private static readonly Dictionary<string, IReadOnlyList<FilterOperator>> AllowedFilters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Total"]     = [FilterOperator.Gt, FilterOperator.Gte, FilterOperator.Lt, FilterOperator.Lte],
        ["CreatedAt"] = [FilterOperator.Gt, FilterOperator.Lt]
    };

    private static readonly HashSet<string> AllowedSortFields = new(["Total", "CreatedAt"], StringComparer.OrdinalIgnoreCase);

    public GetAllOrdersValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("...");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("...");
        RuleFor(x => x.Filters).Must(BeValidFilters).When(x => x.Filters is not null && x.Filters.Count > 0).WithMessage("...");
        RuleFor(x => x.SortFields).Must(BeValidSortFields).When(x => x.SortFields is not null && x.SortFields.Count > 0).WithMessage("...");
    }
    // ... métodos privados de validación
}
```

### Paso 4 — Exponer en el Controller

Los parámetros estándar (`pageNumber`, `pageSize`, `sort`) se declaran con `[FromQuery]` en la firma para que sean visibles en Swagger. Los filtros dinámicos se parsean manualmente del `IQueryCollection`.

```csharp
[HttpGet]
[Authorize(Policy = "orders.read")]
[PaginatedEndpoint(
    AllowedFilters = "total[gt],total[gte],total[lt],total[lte],createdAt[gt],createdAt[lt]",
    AllowedSortFields = "total,createdAt",
    DefaultSort = "-createdAt")]
public async Task<IActionResult> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25,
    [FromQuery] string? sort = null)
{
    var filters = QueryStringFilterParser.ParseFilters(HttpContext.Request.Query);
    var sortFields = QueryStringFilterParser.ParseSortFields(sort);
    var query = new GetAllOrdersQuery(pageNumber, pageSize, filters, sortFields.Count > 0 ? sortFields : null);
    var result = await _mediator.SendQueryAsync(query);
    return Ok(PagedEnvelope<OrderDto>.FromPagedResult(result));
}
```

---

## 8. Reglas

- El tamaño máximo de página (`pageSize`) es **100**. El default es **25**.
- Los filtros se combinan con **AND** lógico.
- Campos de filtro y ordenamiento deben validarse contra **whitelist** — previene inyección SQL.
- `QueryStringFilterParser` vive en la capa **Api** — Application no conoce `HttpContext`.
- `GetPagedAsync` reside en `IGenericRepository<T>` — cualquier repositorio lo hereda automáticamente.
- Los endpoints **no paginados** (detalle por ID, mutaciones) **no** usan envelope.
- Los parámetros estándar (`pageNumber`, `pageSize`, `sort`) **DEBEN** declararse con `[FromQuery]` en la firma del controller para visibilidad en Swagger/OpenAPI.
- Si la entidad tiene un sort por defecto (ej: `-createdAt`), el **handler** lo aplica cuando `SortFields` es null o vacío — no el controller ni el parser.

---

## 9. Swagger/OpenAPI

Los endpoints paginados deben ser completamente descubribles desde Swagger UI.

### Parámetros estándar

`pageNumber`, `pageSize` y `sort` se declaran con `[FromQuery]` en la firma del endpoint:

```csharp
public async Task<IActionResult> GetAll(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25,
    [FromQuery] string? sort = null)
```

Esto hace que Swagger UI muestre campos editables con los valores por defecto.

### Filtros dinámicos

Los filtros con sintaxis `campo[operador]=valor` no pueden representarse con `[FromQuery]`. Se documentan mediante:

1. **Atributo `[PaginatedEndpoint]`:** Se aplica al método del controller. Declara los filtros permitidos, campos de ordenamiento y sort por defecto.

```csharp
[PaginatedEndpoint(
    AllowedFilters = "name[contains],price[gte],price[lte],stock[gt]",
    AllowedSortFields = "name,price,stock,createdAt",
    DefaultSort = "-createdAt")]
```

2. **`PaginatedEndpointOperationFilter`:** Registrado en `SwaggerExtensions.AddSwaggerConfiguration()`. Detecta el atributo y enriquece la descripción del endpoint con la sintaxis de filtros, campos permitidos y ejemplos de uso.

### Cómo agregar a un nuevo endpoint paginado

1. Agregar `[PaginatedEndpoint(...)]` al método del controller con los filtros y sort fields de la entidad.
2. El `PaginatedEndpointOperationFilter` genera la documentación automáticamente.

---

## Documentación Relacionada

- [**DATA_ACCESS.md**](DATA_ACCESS.md) — Implementación de `GetPagedAsync` en repositorio
- [**PATTERNS.md**](PATTERNS.md) — Patrón completo de Query paginada
- [**TESTING.md**](TESTING.md) — Cómo testear handlers y validators paginados
