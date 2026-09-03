---
name: 'Testing Handlers'
description: 'Pruebas de CQRS Handlers.'
applyTo: 'tests/**/Handlers/**/*Tests.cs'
---
# Tests de Handlers
- Nombrado: `Handle_Should_Result_When_Scenario` (el método real se llama `Handle`, no `HandleAsync`).
- Inicializar mocks y handler en **constructor** con campos `readonly` (best practice MSTest .NET 10).
- Mockear repositorios y UnitOfWork. Nunca inyectar reales.
- Comprobar side effects con `Verify`:
  - `_unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);`
  - `_unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);`
- Para excepciones usar patrón `Func<Task> act = async () => await _handler.Handle(...); await act.Should().ThrowAsync<T>();`.
- Handlers de query: verificar que no interactúan con DB si hacen hit de caché.

## Tests de Query Handlers Paginados
- Mockear `IXxxRepository.GetPagedAsync(...)` → retorna tupla `(data, totalCount)`.
- Verificar que `PagedResult<TDto>` tiene `Data.Count()`, `TotalCount`, `PageNumber`, `PageSize` correctos.
- Verificar mapeo entidad → DTO campo a campo.
- Verificar que filtros y sort se pasan al repositorio (capturar con `Callback` y `It.IsAny`).
- Verificar caso vacío: `Data` vacío, `TotalPages == 0`.
- **Referencia**: `tests/Olimpia.Tests/Handlers/Products/GetAllProductsHandlerTests.cs`.
