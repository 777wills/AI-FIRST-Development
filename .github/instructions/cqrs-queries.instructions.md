---
name: 'CQRS Queries'
description: 'Patrones para Queries y Query Handlers.'
applyTo: 'src/**/Queries/**/*Query.cs,src/**/Queries/**/*Handler.cs'
---
# CQRS Queries (Lectura)
- **Framework**: `Cortex.Mediator`. Usar namespaces `Cortex.Mediator.Queries`.
- **Query**: Declarado como `record` inmutable (ej. `record GetProductByIdQuery(int Id) : IQuery<ProductDto>;`).
- **Handler**: Implementa `IQueryHandler<TQuery, TResult>`.
- **Dispatch**: Las queries se despachan con `SendQueryAsync()` (NO `SendAsync()`). Ejemplo: `var result = await _mediator.SendQueryAsync(new GetProductByIdQuery(id));`
- **Transacciones**: Las queries NO usan `IUnitOfWork` ni abren transacciones explícitas.
- **No Entities**: Retornar DTOs, no las entidades del dominio directamente.

## Queries Paginadas
- Heredar de `PagedQuery` (abstract record en `Application/Common/Pagination/`).
- Implementar `IQuery<PagedResult<TDto>>`.
- El handler llama a `_repository.GetPagedAsync(...)`, mapea a DTOs y retorna `PagedResult<TDto>.Create(...)`.
- En el controller, envolver con `PagedEnvelope<TDto>.FromPagedResult(result)`.
- **Referencia**: `src/Olimpia.Application/Products/Queries/GetAllProducts/`.
