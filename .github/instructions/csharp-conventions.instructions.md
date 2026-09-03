---
name: 'Convenciones C#'
description: 'Reglas de estilo C# obligatorias para todo el código del proyecto.'
applyTo: '**/*.cs'
---
# Convenciones C# (Olimpia)

Estas reglas son **obligatorias** en todo el código. El código existente debe ajustarse al editarse. Los analyzers en `.editorconfig` enforcean las reglas automatizables.

## A1. Formato y espaciado
- Al menos una línea en blanco entre definiciones de métodos.
- Al menos una línea en blanco entre definiciones de propiedades cuando tengan docs XML, inicializadores o lógica.
- No usar `catch` en una sola línea: abrir llaves en línea nueva siempre.

## A2. Idioma
- Identificadores, nombres de tipos y variables en **inglés**.
- Mensajes de excepción de negocio (`throw new ... ("...")`) en **español** (destinados a usuarios finales).
- Comentarios de código en **español**.
- Prohibido emojis en comentarios de código o logs de infraestructura.

## A3. Comentarios
- Comentarios en **línea separada** (no al final de una línea de código).
- Iniciar con **mayúscula** y terminar con **punto**.
- Explican el *porqué*, no el *qué* (el identificador ya describe el qué).

## A4. Interfaces
- Nombres de interfaces con prefijo **`I`** (`IProductRepository`, `IExternalApiClient`).

## A5. Abreviaturas
- **Minimizar** abreviaturas. Cuando se usen, relación **1↔1** (una abreviatura = un significado).
- Abreviaturas aprobadas (PascalCase .NET estándar):

| Término | Forma correcta | Incorrecta |
|---------|----------------|------------|
| Identifier | `Id` | `ID`, `Identif` |
| Uniform Resource Locator | `Url` | `URL` |
| HTTP | `Http` | `HTTP` |
| SQL | `Sql` | `SQL` |
| Application Programming Interface | `Api` | `API` |
| Globally Unique Identifier | `Guid` | `GUID` |
| Input / Output | `Io` | `IO` |

- Cuando una sigla arranca un identificador en camelCase (parámetro o campo), va toda en minúscula: `htmlBody`, `urlBuilder`, `apiClient`.

## A6. Nombres de clases y archivos
- **Una clase pública por archivo**.
- El nombre del **archivo fuente** coincide con el nombre de la clase (`Product.cs` contiene `class Product`).

## A7. Nombres de propiedades
- No repetir el nombre de la clase en la propiedad: `Rectangle.Area` (no `Rectangle.RectangleArea`).
- `PascalCase` en propiedades; `_camelCase` en campos privados.

## A8. Nombres de booleanos
- Prefijos semánticos obligatorios:
  - **`Is`** para estado (`IsEnabled`, `IsValid`).
  - **`Has`** para posesión (`HasPermission`, `HasItems`).
  - **`Can`** para capacidad (`CanEdit`, `CanRetry`).
  - **`Should`** para recomendación (`ShouldRetry`).

## A9. Nombres de métodos
- Nombrar por **acción + entidad**: `ReadFile`, `GetBalance`, `GetSummary`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.
- Si el método retorna valor, el nombre debe indicar **qué se devuelve** (`GetProductById`, `GetUserSummary`).

## A10. Sellado de clases
- Clases concretas deben ser **`sealed`** salvo que estén diseñadas para herencia.
- Static classes: idealmente `static sealed` para enfatizar intención.

## A11. Comparaciones booleanas
- **NUNCA** comparar booleanos contra `true` o `false`:
  - ❌ `if (isValid == true)` → ✅ `if (isValid)`
  - ❌ `if (isValid == false)` → ✅ `if (!isValid)`
  - ❌ `if (x?.Y == true)` → ✅ `if (x is { Y: true })` o condición equivalente.

## A12. Liberación explícita de referencias
- Toda instancia que implemente `IDisposable` debe liberarse con `using` o `using var`.
- Cuando un tipo posee recursos desechables como campos, debe implementar `IDisposable` (analyzer CA2213).
- **Excepción justificada**: patrón fire-and-forget de `CustomLogger.SendLogAsync`. Está documentado como excepción explícita por performance (ver A11 en `docs/PATTERNS.md`). Cualquier otra excepción debe documentarse localmente con `<remarks>` XML.

## A13. Scope de variables
- Declarar variables **lo más cerca posible** de su primer uso.
- Preferir `readonly` / `const` donde el valor no cambie.
- No "levantar" variables al inicio del método si sólo se usan dentro de un `if`/bloque.

## A14. Prohibición de `null` returns
- **Los métodos no deben retornar `null` de forma "sorpresa".** El consumidor no debería necesitar null-check en cada llamado.
- Patrones sustitutos según semántica:
  - **"No encontrado" = error** → lanzar `KeyNotFoundException` y tipar `Task<T>` (no `Task<T?>`).
  - **"No encontrado" = caso válido** → usar `bool TryGetX(..., out T value)` o `Result<T>` / `Option<T>`.
  - **Recursos opcionales** (ej. `BeginScope`) → devolver un objeto null-object (`NullScope.Instance`) en vez de `null`.
- Si la firma usa `T?`, el consumidor **debe** tratar el `null` explícitamente (nunca propagarlo sin pensar).

## A15. Especificidad de tipos ante colisiones
- En repositorios, handlers y tests calificar completamente tipos de entidad con `global::` para evitar colisiones:
  - `global::Olimpia.Domain.Entities.Product`
- Aplica cuando un namespace local (ej. `Olimpia.Application.Products`) contiene tipos con el mismo nombre que la entidad del Domain.

## A16. Casting
- **Preferir** `is` + pattern matching o `as` + null-check:
  - ✅ `if (x is Foo f) { ... }`
  - ✅ `var f = x as Foo; if (f is null) { ... }`
- Cast explícito `(Foo)x` permitido **sólo** si:
  1. El contrato estático garantiza la conversión (p. ej. genéricos con `where T : Foo`), o
  2. Se documenta con un comentario inmediato anterior justificando el cast y por qué es seguro.
- Evitar casts implícitos que puedan perder precisión (`decimal` → `int`, `long` → `int`).

## A17. Uso de `var`
- `var` **sólo** cuando el tipo es evidente del lado derecho (`var count = 0;`, `var list = new List<Product>();`).
- En retornos de métodos no triviales, declarar tipo explícito para legibilidad.

## A18. Nota para código generado por IA
- Todo método generado por GitHub Copilot incluye `// Método generado por GitHub Copilot` al inicio.
- Bloques grandes generados/refactorizados se delimitan con `// Inicio código generado por GitHub Copilot` y `// Fin código generado por GitHub Copilot`.
- Refactorizaciones se delimitan con `// Inicio refactorización/optimización por GitHub Copilot` y `// Fin refactorización/optimización por GitHub Copilot`.
- Regla mantenida por decisión del equipo (ver `.github/copilot-instructions.md`).

## A19. Timestamps: siempre en hora local del servidor
- **NUNCA** usar `DateTime.UtcNow` en código de producción.
- Usar siempre **`DateTime.Now`** para generar timestamps; el servidor DEBE tener configurada
  la zona horaria del negocio (Colombia / Perú — UTC-5).
- Los valores `DateTime` almacenados en BD representan **hora local** (`DateTimeKind.Unspecified`
  o `DateTimeKind.Local`). NUNCA almacenar UTC a menos que una integración externa lo requiera
  (documentar la razón).
- `BaseEntity.CreatedAt` se inicializa con `DateTime.Now`.
  `GenericRepository.UpdateAsync` asigna `UpdatedAt = DateTime.Now` antes de persistir.
- En datos de prueba, usar `DateTime.Now` o `new DateTime(y, m, d, h, min, s, DateTimeKind.Unspecified)`.
  NUNCA `DateTimeKind.Utc` en datos de prueba de entidades del dominio.
