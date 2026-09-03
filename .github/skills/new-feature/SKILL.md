---
name: new-feature
description: "Checklist end-to-end para implementar una nueva feature completa en Olimpia: entidad, repositorio, CQRS, validación, controller y tests."
---

# Skill: Implementar Nueva Feature — Checklist

Guía paso a paso para agregar una feature completa en Olimpia (Clean Architecture + CQRS + TDD).
Las reglas detalladas de cada capa están en las instructions que se auto-cargan por `applyTo`.

---

## Paso 1 — Entidad en Domain

- **Crear:** `src/Olimpia.Domain/Entities/{Entidad}.cs`
- Heredar de `BaseEntity` (provee `Id`, `CreatedAt`, `UpdatedAt`).
- `sealed`, propiedades PascalCase, constructor dual (vacío para Dapper + parametrizado).
- **Referencia:** `src/Olimpia.Domain/Entities/Product.cs`
- **Convenciones:** `domain-entities.instructions.md`

## Paso 2 — Interfaz de Repositorio en Domain

- **Crear:** `src/Olimpia.Domain/Repositories/I{Entidad}Repository.cs`
- Heredar de `IGenericRepository<T>` — solo agregar métodos de dominio específicos.
- **Referencia:** `src/Olimpia.Domain/Repositories/IProductRepository.cs`

## Paso 3 — Repositorio en Infrastructure

- **Crear:** `src/Olimpia.Infrastructure/Persistence/Repositories/{Entidad}Repository.cs`
- Heredar de `GenericRepository<T>`, implementar la interfaz del paso 2.
- Usar `global::Olimpia.Domain.Entities.{Entidad}` en toda referencia al tipo.
- Pasar `transaction: UnitOfWork.DbTransaction` en toda consulta SqlKata.
- No reimplementar CRUD — auto-registro lo detecta.
- **Referencia:** `src/Olimpia.Infrastructure/Persistence/Repositories/ProductRepository.cs`
- **Convenciones:** `data-access.instructions.md`

## Paso 4 — Command + Handler en Application

- **Crear carpeta:** `src/Olimpia.Application/{Feature}/Commands/{Accion}/`
- **Archivos:** `{Accion}Command.cs` (record, `ICommand<T>`) + `{Accion}Handler.cs` (sealed, `ICommandHandler<,>`)
- Handler usa `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` con try/catch.
- Lanza excepciones de negocio: `InvalidOperationException`, `KeyNotFoundException`, `ArgumentException`.
- **Referencia:** `src/Olimpia.Application/Products/Commands/CreateProduct/`
- **Convenciones:** `cqrs-handlers.instructions.md`

## Paso 5 — Query + Handler + DTO en Application

- **Crear carpeta:** `src/Olimpia.Application/{Feature}/Queries/{Accion}/`
- **Archivos:** `{Accion}Query.cs` (record, `IQuery<T>`) + `{Accion}Handler.cs` + `{Entidad}Dto.cs` (record)
- Query Handlers no abren transacción.

### Variante: Query Paginada (listados)

- Heredar de `PagedQuery` (abstract record en `Application/Common/Pagination/`) e implementar `IQuery<PagedResult<TDto>>`.
- El handler llama a `_repository.GetPagedAsync(...)`, mapea a DTOs, retorna `PagedResult<TDto>.Create(...)`.
- El controller envuelve con `PagedEnvelope<TDto>.FromPagedResult(result)`.
- Definir whitelist de campos filtrables/ordenables en el Validator.
- **Referencia:** `src/Olimpia.Application/Products/Queries/GetAllProducts/`
- **Convenciones:** `api-pagination.instructions.md`

## Paso 6 — Validator con FluentValidation

- **Crear:** en la misma carpeta del Command: `{Command}Validator.cs`
- `sealed class`, hereda `AbstractValidator<T>`.
- Un validador por Command/Query.

## Paso 7 — Controller en API

- **Crear:** `src/Olimpia.Api/Controllers/V1/{Feature}Controller.cs`
- Namespace: `Olimpia.Api.Controllers.V1` (alineado con la carpeta).
- Clase: `public sealed class {Feature}Controller : ApiController` (NO partial).
- `[ApiVersion("1.0")]` a nivel de clase (con `using Asp.Versioning;`), formato major.minor.
- `[MapToApiVersion("1.0")]` en cada método de endpoint.
- Inyectar `IMediator`, usar `SendAsync`.
- try/catch para capturar excepciones del Handler.
- `[Authorize]` a nivel de clase, `[Authorize(Policy = "...")]` en endpoints de escritura.
- **El versionado es exclusivo de la capa API.** Application, Domain e Infrastructure NO se versionan.
- **Referencia:** `src/Olimpia.Api/Controllers/V1/ProductController.cs`
- **Convenciones:** `api-controllers.instructions.md`

## Paso 8 — Tests (TDD)

- **Crear:** `tests/Olimpia.Tests/{Feature}/`
- Un `[TestClass]` por Handler o Validator, todas `sealed`.
- Naming: `Metodo_Should_Resultado_When_Condicion`
- Patrón AAA, `global::` para entidades, solo mocks de interfaces.
- Verificar `CommitAsync` (éxito) y `RollbackAsync` (fallo) en Command Handlers.
- **Referencia:** `tests/Olimpia.Tests/Products/Commands/CreateProductTests.cs`
- **Convenciones:** `testing.instructions.md` + skill `tdd-workflow`

## Paso 9 — Script SQL

- **Crear:** `scripts/{NombreDescriptivo}.sql`
- Iniciar con `USE [APIBase]; GO`.
- Documentar tabla y columnas con `sp_addextendedproperty`.
- Idempotente (`IF NOT EXISTS` antes de crear).
- Nombre de tabla = `{Entidad}s` (PascalCase plural).
- **Convenciones:** `database.instructions.md`

---

## Checklist Final

- [ ] Entidades `sealed`, heredan `BaseEntity`, constructor dual
- [ ] Interfaz extiende `IGenericRepository<T>`, solo métodos específicos
- [ ] Repositorio usa `global::` y `transaction: UnitOfWork.DbTransaction`
- [ ] Command/Query son `record` con `ICommand<T>` / `IQuery<T>`
- [ ] Handler try/catch con `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`
- [ ] Handler inyecta interfaces, nunca concretos
- [ ] Validator `AbstractValidator<T>` en misma carpeta del Command
- [ ] Controller hereda `ApiController`, usa `SendAsync`, try/catch
- [ ] `[Authorize]` a nivel de clase, policies en escritura
- [ ] Tests: naming `Metodo_Should_Resultado_When_Condicion`, AAA, `global::`
- [ ] Script SQL: `USE [APIBase]`, `sp_addextendedproperty`, idempotente
- [ ] `[ApiVersion("1.0")]` en la clase controller (major.minor)
- [ ] `[MapToApiVersion("1.0")]` en cada endpoint
- [ ] Controller en `Controllers/V1/` con namespace `Olimpia.Api.Controllers.V1`
- [ ] `public sealed class` (NO partial)
- [ ] DI: sin registro manual (auto-scan detecta repos, handlers y validators)
