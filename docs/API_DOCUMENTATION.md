# 📖 Documentación XML de la API — Olimpia

Guía para humanos sobre la documentación XML obligatoria en Controllers, Commands, Queries y DTOs expuestos al cliente (Swagger/OpenAPI). La versión normativa para agentes IA vive en [`.github/instructions/api-xmldocs.instructions.md`](../.github/instructions/api-xmldocs.instructions.md).

---

## Tabla de contenidos

- [1. Alcance](#1-alcance)
- [2. Reglas de Controllers](#2-reglas-de-controllers)
- [3. Reglas de Commands / Queries / DTOs](#3-reglas-de-commands--queries--dtos)
- [4. Manejo centralizado de errores (ProblemDetails)](#4-manejo-centralizado-de-errores-problemdetails)
- [5. Example values en Swagger](#5-example-values-en-swagger)
- [6. Omitir un endpoint del Swagger](#6-omitir-un-endpoint-del-swagger)
- [7. Before / After](#7-before--after)
- [8. Verificación](#8-verificación)

---

## 1. Alcance

| Obligatorio | No obligatorio |
|-------------|----------------|
| Acciones públicas de Controllers | Handlers (`CreateProductHandler`) |
| `CreateXCommand`, `GetXQuery` records | Validators |
| DTOs de respuesta (`ProductDto`) | Repositorios |
| Enums públicos usados en contratos | Servicios internos |
| Cualquier tipo referenciado desde el anterior | Interfaces del Domain |

Regla general: **si aparece en Swagger, lleva XML**.

---

## 2. Reglas de Controllers

Cada acción del Controller debe tener:

1. `<summary>` — una frase imperativa.
2. `<remarks>` — reglas de negocio, scopes requeridos, idempotencia, etc.
3. `<param name="...">` por cada parámetro (path, query, body).
4. `<returns>` — descripción interna (no aparece en Swagger pero sirve para devs).
5. `<response code="XXX">` por cada código HTTP posible.
6. `[ProducesResponseType(...)]` **equivalente** a cada `<response>`.

### Códigos HTTP a documentar

| Código | Uso | Tipo de respuesta |
|--------|-----|-------------------|
| 200 OK | Éxito en GET/PUT | `typeof(TDto)` |
| 201 Created | Éxito en POST que crea | `typeof(TDto)` |
| 204 NoContent | Éxito en DELETE | sin body |
| 400 BadRequest | Validación fallida, payload malformado | `typeof(ProblemDetails)` |
| 401 Unauthorized | Sin token o token inválido | `typeof(ProblemDetails)` |
| 403 Forbidden | Sin scope/policy | `typeof(ProblemDetails)` |
| 404 NotFound | Recurso no existe | `typeof(ProblemDetails)` |
| 409 Conflict | Duplicado o estado inválido | `typeof(ProblemDetails)` |
| 500 InternalServerError | Error no esperado | `typeof(ProblemDetails)` |

**No implementar try/catch en el Controller**: el `ExceptionHandlingMiddleware` traduce excepciones a su código HTTP (ver sección 4).

### Endpoints paginados

Los endpoints con `[PaginatedEndpoint(...)]` deben documentar:
- `pageNumber`, `pageSize`, `sort` como `[FromQuery]`.
- Filtros dinámicos (`campo[operador]=valor`) en `<remarks>` — Swagger los descubre vía `PaginatedEndpointOperationFilter`, pero el humano los quiere legibles.

---

## 3. Reglas de Commands / Queries / DTOs

Cada record/clase expuesto debe tener:

1. `<summary>` en el tipo (describe el caso de uso).
2. `<summary>` en cada propiedad pública.
3. `<example>` opcional en propiedades con formato no obvio.
4. `[Required]` en propiedades obligatorias no-nullable.

### Sintaxis de records

Dos opciones:

**A. Record posicional** (records short-form) — documentar en `<param>` del summary del record:

```csharp
/// <summary>
/// Comando para crear un producto en el catálogo.
/// </summary>
/// <param name="Name">Nombre del producto. Máx 100 caracteres.</param>
/// <param name="Description">Descripción opcional. Máx 500 caracteres.</param>
/// <param name="Price">Precio en USD. Debe ser mayor a 0.</param>
/// <param name="Stock">Cantidad inicial en stock.</param>
public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock) : ICommand<int>;
```

**B. Record con propiedades inicializables** — XML y `[Required]` por propiedad:

```csharp
/// <summary>
/// Comando para crear un producto en el catálogo.
/// </summary>
public sealed record CreateProductCommand : ICommand<int>
{
    /// <summary>Nombre del producto. Máximo 100 caracteres.</summary>
    /// <example>Laptop Dell XPS 15</example>
    [Required]
    public required string Name { get; init; }

    /// <summary>Descripción opcional.</summary>
    public string? Description { get; init; }

    /// <summary>Precio en USD. Mayor a 0.</summary>
    /// <example>1499.99</example>
    [Required]
    public required decimal Price { get; init; }

    /// <summary>Cantidad inicial en stock.</summary>
    [Required]
    public required int Stock { get; init; }
}
```

Usar **B** cuando se necesite `[Required]` o `<example>` por propiedad. Usar **A** en records triviales.

---

## 4. Manejo centralizado de errores (ProblemDetails)

A partir de esta versión, **los Controllers no llevan try/catch**. Un middleware global traduce las excepciones a `ProblemDetails` tipado.

### Mapeos

| Excepción | Status | Título |
|-----------|--------|--------|
| `ValidationException` (FluentValidation) | 400 | "Validación fallida" |
| `ArgumentException` | 400 | "Argumento inválido" |
| `UnauthorizedAccessException` | 401 | "No autorizado" |
| `KeyNotFoundException` | 404 | "Recurso no encontrado" |
| `InvalidOperationException` | 409 | "Conflicto" |
| (resto) | 500 | "Error interno" |

### ProblemDetails extendido

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Conflicto",
  "status": 409,
  "detail": "Ya existe un producto con el nombre 'Laptop Dell XPS 15'.",
  "instance": "/api/v1/products",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

### Documentación del endpoint

```csharp
/// <response code="200">Producto creado.</response>
/// <response code="400">Validación fallida.</response>
/// <response code="409">Ya existe un producto con ese nombre.</response>
[ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> Create(CreateProductCommand command)
{
    var id = await _mediator.SendAsync(command);
    return Ok(new CreateProductResponse(id));
}
```

---

## 5. Example values en Swagger

- **Obligatorio** en propiedades con formato no obvio:
  - Identificadores (`/// <example>42</example>`)
  - Códigos (`/// <example>ACME-0001</example>`)
  - Fechas (`/// <example>2026-04-17T08:00:00Z</example>`)
  - Enumerados representados como string
- **Prohibido** en strings libres donde el ejemplo aporte ruido:
  - No escribir `/// <example>Product</example>` en un `Name` porque duplica lo que ya indica el nombre del campo.

---

## 6. Omitir un endpoint del Swagger

Un endpoint que existe en el servicio pero **no debe aparecer en Swagger** se marca con:

```csharp
[HttpPost]
[ApiExplorerSettings(IgnoreApi = true)]
public IActionResult InternalOnly(...) { ... }
```

Casos típicos: webhooks internos, endpoints de health/probe que tienen su propia doc, acciones experimentales.

---

## 7. Before / After

### Antes (`ProductController.Create` con try/catch y sin XML)

```csharp
[HttpPost]
[Authorize(Policy = "products.write")]
public async Task<IActionResult> Create(CreateProductCommand command)
{
    try
    {
        var id = await _mediator.SendAsync(command);
        return Ok(new { Id = id });
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new { Error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
```

### Después (middleware + XML + ProducesResponseType)

```csharp
/// <summary>
/// Crea un producto en el catálogo.
/// </summary>
/// <remarks>
/// Requiere scope 'products.write'. El nombre del producto debe ser único.
/// </remarks>
/// <param name="command">Datos del producto a crear.</param>
/// <returns>Identificador del producto recién creado.</returns>
/// <response code="200">Producto creado.</response>
/// <response code="400">Validación fallida.</response>
/// <response code="401">No autenticado.</response>
/// <response code="403">Sin scope 'products.write'.</response>
/// <response code="409">Ya existe un producto con ese nombre.</response>
/// <response code="500">Error interno.</response>
[MapToApiVersion("1.0")]
[HttpPost]
[Authorize(Policy = "products.write")]
[ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Create(CreateProductCommand command)
{
    var id = await _mediator.SendAsync(command);
    return Ok(new CreateProductResponse(id));
}
```

---

## 8. Verificación

Al terminar de documentar un endpoint:

1. `dotnet build` compila sin warnings de analyzer (`SA1600`, `CS1591`).
2. `dotnet run --project src/Olimpia.Api` arranca la API.
3. Abrir `/swagger`:
   - La acción muestra `summary` + `description` correctos.
   - Cada parámetro tiene descripción.
   - Todas las respuestas declaradas aparecen (200/400/401/404/409/500).
   - El schema de `ProblemDetails` se referencia en las respuestas de error.
4. Abrir `/swagger/v1/swagger.json` y revisar:
   - `paths.{endpoint}.responses` contiene los códigos esperados.
   - `components.schemas.CreateProductCommand.properties.{name}.description` tiene texto.

---

## Referencias

- [`.github/instructions/api-xmldocs.instructions.md`](../.github/instructions/api-xmldocs.instructions.md) — versión normativa para IAs.
- [`.github/instructions/csharp-conventions.instructions.md`](../.github/instructions/csharp-conventions.instructions.md) — convenciones generales.
- [`docs/PATTERNS.md`](PATTERNS.md) — patrones CQRS y Controllers.
- [`docs/AUTHENTICATION.md`](AUTHENTICATION.md) — scopes y políticas.
