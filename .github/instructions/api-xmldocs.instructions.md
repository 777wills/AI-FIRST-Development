---
name: 'Documentación XML de Api'
description: 'Obligatoriedad y plantilla de comentarios XML en Controllers, Commands, Queries y DTOs expuestos.'
applyTo: 'src/Olimpia.Api/Controllers/**/*.cs, src/Olimpia.Application/**/Commands/**/*Command.cs, src/Olimpia.Application/**/Queries/**/*Query.cs, src/Olimpia.Application/**/*Dto.cs'
---
# Documentación XML de la Api

Toda superficie pública que se proyecte en Swagger/OpenAPI debe estar documentada. Los analyzers (`.editorconfig` con `SA1600`/`1591`) exigen XML en estos archivos.

## Alcance
- **Obligatorio**: acciones de `Controllers`, registros `Command`, `Query`, `Dto` (y clases públicas expuestas vía body/query/response).
- **No obligatorio**: handlers, validators, repositorios, servicios internos, interfaces del Domain.

## Plantilla para acciones de Controller

```csharp
/// <summary>
/// Descripción corta del endpoint en una sola frase.
/// </summary>
/// <remarks>
/// Descripción detallada, pre-requisitos, reglas de negocio, ejemplos de uso.
/// </remarks>
/// <param name="command">Descripción del parámetro de entrada.</param>
/// <returns>Descripción interna (no aparece en Swagger).</returns>
/// <response code="200">Descripción de la respuesta exitosa.</response>
/// <response code="400">Descripción de BadRequest (validación fallida).</response>
/// <response code="401">No autenticado.</response>
/// <response code="404">Recurso no encontrado.</response>
/// <response code="409">Conflicto (ej. entidad duplicada).</response>
/// <response code="500">Error interno.</response>
[HttpPost]
[ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Create(CreateProductCommand command) { ... }
```

### Reglas
- `<summary>` **obligatorio**. Una frase imperativa que describe la acción.
- `<remarks>` **obligatorio** salvo que la acción sea trivial. Contiene reglas de negocio, scopes requeridos, idempotencia, links a documentación.
- `<param>` **obligatorio** para cada parámetro (path, query, body). Usar `example="..."` cuando el formato no sea obvio.
- `<response>` **obligatorio por cada código HTTP posible**. Debe existir un `[ProducesResponseType]` equivalente.
- `[ProducesResponseType]` **obligatorio por cada código HTTP posible**:
  - Respuesta exitosa con el tipo concreto del DTO.
  - Respuestas de error con `typeof(ProblemDetails)` (el middleware global las produce).
- Prohibidos try/catch para mapear excepciones a códigos HTTP: eso lo hace `ExceptionHandlingMiddleware`. Documentar los códigos posibles, no implementar la traducción en la acción.

## Plantilla para Commands / Queries / DTOs

```csharp
/// <summary>
/// Resumen del caso de uso (ej. "Comando para crear un producto.").
/// </summary>
/// <param name="Name">Nombre del producto.</param>
/// <param name="Price">Precio del producto en la moneda por defecto.</param>
public sealed record CreateProductCommand(
    /// <summary>Nombre del producto. Máximo 100 caracteres.</summary>
    /// <example>Laptop Dell XPS 15</example>
    [Required]
    string Name,

    /// <summary>Descripción detallada del producto.</summary>
    string? Description,

    /// <summary>Precio del producto en USD. Debe ser mayor a 0.</summary>
    /// <example>1499.99</example>
    [Required]
    decimal Price,

    /// <summary>Cantidad inicial en stock.</summary>
    /// <example>10</example>
    [Required]
    int Stock) : ICommand<int>;
```

### Reglas
- `<summary>` **obligatorio** en el record/clase y en cada propiedad pública.
- `<example>` **opcional pero recomendado** en propiedades cuyo formato no sea evidente: IDs, códigos, formatos especiales, rangos significativos. Prohibido en strings libres obvios (`Name = "Product"` no aporta).
- `[Required]` **obligatorio** en propiedades que no sean nullable y que no tengan valor por defecto (Swagger las marca como requeridas).
- Cuando el record use sintaxis posicional (`record X(int Id)`), documentar con `<param name="Id">` en el summary del record, o usar sintaxis tradicional con propiedades inicializables para poder ponerle XML a cada una.

## Omitir documentación / endpoints internos
- Para excluir un endpoint del Swagger sin removerlo del servicio:
  ```csharp
  [ApiExplorerSettings(IgnoreApi = true)]
  ```
- Omitir es **excepción**; la regla es documentar.

## Verificación
- `dotnet build` debe activar generación de XML (`<GenerateDocumentationFile>true</GenerateDocumentationFile>` en el `.csproj` de `Olimpia.Api` y `Olimpia.Application`).
- El archivo `{proyecto}.xml` de cada proyecto se incluye en Swagger via `c.IncludeXmlComments(...)`.
- Al inspeccionar `/swagger/v1/swagger.json`, cada endpoint debe mostrar `description`, `parameters[].description`, y `responses` con los códigos declarados.
