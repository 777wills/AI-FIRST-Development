# Olimpia — Reglas Globales y Críticas (GitHub Copilot)

Estas reglas aplican a TODO el código. El resto del contexto se cargará dinámicamente según el archivo editado (`.github/instructions/`) o bajo demanda vía Agent Skills (`.github/skills/`).

## Referencias clave (cargar según contexto)

- [`instructions/csharp-conventions.instructions.md`](instructions/csharp-conventions.instructions.md) — Convenciones A1–A19 (formato, idioma, naming, `sealed`, prohibición de nulls, `is/as`, `var`, timestamps con DateTime.Now).
- [`instructions/api-xmldocs.instructions.md`](instructions/api-xmldocs.instructions.md) — XML docs obligatorias en Controllers, Commands, Queries y DTOs expuestos, con `[ProducesResponseType]` por cada código HTTP posible.
- [`../docs/API_DOCUMENTATION.md`](../docs/API_DOCUMENTATION.md) — Guía de `ProblemDetails` y manejo centralizado de errores. **Los Controllers no llevan try/catch**; el `ExceptionHandlingMiddleware` traduce excepciones.
- [`../docs/PATTERNS.md §7`](../docs/PATTERNS.md) — Code Style narrativo para humanos.
- [`../docs/TESTING.md`](../docs/TESTING.md) — MSTest + Moq + FluentAssertions, un assert lógico (`BeEquivalentTo`), `[DataRow]`.

## Stack Base y Arquitectura
- **Stack**: `.NET 10`, `C# 13`.
- **Arquitectura**: Clean Architecture estricta.
  - `Domain` no tiene dependencias externas.
  - `Application` depende solo de `Domain`.
  - `Infrastructure` y `Api` dependen de `Application`.

## Prohibiciones Absolutas
- **NO Entity Framework**: El acceso a datos usa exclusivamente `Dapper` y `SqlKata`.
- **NO MediatR**: Usar exclusivamente `Cortex.Mediator` (`SendAsync` para Commands, `SendQueryAsync` para Queries).
- **NO SQL Crudo**: Usar siempre la API fluida de SqlKata o repositorios de Stored Procedures/Views.
- **NO Clases Concretas en Inyección**: Inyectar siempre interfaces.

## Estructura del Proyecto
- `src/Olimpia.Domain/` — Entidades, interfaces de repositorio (sin dependencias externas).
- `src/Olimpia.Application/` — CQRS: Commands, Queries, Handlers, Validators (depende solo de Domain).
- `src/Olimpia.Infrastructure/` — Repositorios (Dapper+SqlKata), DI (Scrutor), HTTP Clients (depende de Application).
- `src/Olimpia.Api/` — Controllers, Middleware, Program.cs (depende de Application, Infrastructure).
- `src/Olimpia.Api.Gateway/` — API Gateway con Ocelot + MMLib (token relay transparente, SwaggerForOcelot).
- `tests/Olimpia.Tests/` — Tests unitarios (MSTest + Moq + FluentAssertions).

## Reglas Generales
- **Uso de `global::`**: En repositorios, handlers y tests, calificar completamente los tipos de entidad (ej. `global::Olimpia.Domain.Entities.Product`) para evitar colisiones de namespace.
- **Idioma**: Identificadores, nombres de tipos y variables en inglés. Mensajes de excepción de negocio (`throw`) en español (destinados a usuarios finales). Comentarios de código en español.
- **Prohibición de `null` returns**: Métodos nunca devuelven `null` de forma sorpresa. Patrones sustitutos: lanzar excepción (`KeyNotFoundException`) cuando "no encontrado" es error, `bool TryGetX(out T)` o `Result<T>` cuando "no encontrado" es caso válido, null-object (`NullScope.Instance`) para recursos opcionales. Si la firma usa `T?`, el consumidor debe tratarlo explícitamente. Ver A14 en [`instructions/csharp-conventions.instructions.md`](instructions/csharp-conventions.instructions.md).
- **Abreviaturas en PascalCase .NET**: `Id`, `Url`, `Http`, `Sql`, `Api`, `Guid`, `Io` (NUNCA `ID`, `URL`, `HTTP`, `SQL`, `API`). Siglas al inicio de identificador camelCase van en minúscula: `apiClient`, `htmlBody`.
- **`sealed` en clases concretas** salvo que estén diseñadas para herencia.
- **NO `== true` / `== false`**: usar la variable booleana directamente o `!`.
- **Controllers sin try/catch**: el `ExceptionHandlingMiddleware` traduce excepciones a `ProblemDetails`. Documentar los códigos HTTP posibles con `<response>` + `[ProducesResponseType]`.
- **Identificación de Métodos Generados**: Todo método generado por GitHub Copilot debe incluir un comentario al inicio que indique claramente que fue generado por Copilot. Ejemplo: `// Método generado por GitHub Copilot`
- **Marcado de Fragmentos de Código**: Cuando se genere un fragmento de código con GitHub Copilot, se debe indicar el inicio y el fin del fragmento con comentarios claros, por ejemplo:
   // Inicio código generado por GitHub Copilot
   ...código...
   // Fin código generado por GitHub Copilot
- **Documentación de Refactorizaciones y Optimizaciones**: Toda refactorización u optimización de código realizada por medio de GitHub Copilot debe incluir un comentario al inicio y al final del bloque indicando que fue realizada por Copilot. Ejemplo:
   // Inicio refactorización/optimización por GitHub Copilot
   ...código refactorizado...
   // Fin refactorización/optimización por GitHub Copilot

## Mantenimiento de Arquitectura

Cualquier cambio en la arquitectura base del proyecto (nuevos proyectos/assemblies, nuevos patrones, cambios en middleware, actualización de paquetes clave, cambios en la estructura de capas) **obliga** a actualizar los siguientes artefactos antes de cerrar la tarea:

- **`docs/`** — Actualizar el documento afectado: `ARCHITECTURE.md` si cambia la estructura, `PATTERNS.md` si se agrega/modifica un patrón, `AUTHENTICATION.md` si cambia auth, etc.
- **Este archivo (`copilot-instructions.md`)** — Actualizar la sección "Estructura del Proyecto" si se agregan o eliminan proyectos/assemblies.
- **SpecKit constitution** (`.specify/memory/constitution.md`) — Actualizar el principio afectado (I–IX), bumpear la versión semver y registrar la fecha de enmienda. Este es el documento de gobierno vinculante del proyecto.

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
