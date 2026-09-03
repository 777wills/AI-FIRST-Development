---
name: 'CQRS Commands'
description: 'Patrones para Commands y Command Handlers.'
applyTo: 'src/**/Commands/**/*Command.cs,src/**/Commands/**/*Handler.cs'
---
# CQRS Commands (Escritura)
- **Framework**: `Cortex.Mediator`. Usar namespaces `Cortex.Mediator.Commands`.
- **Command**: Declarado como `record` inmutable (ej. `record CreateProductCommand(...) : ICommand<int>;`).
- **Handler**: Implementa `ICommandHandler<TCommand, TResult>`.
- **Transacciones**: Handlers de escritura SIEMPRE inyectan `IUnitOfWork` y envuelven la persistencia en `BeginTransactionAsync`, `CommitAsync` y `RollbackAsync` con `try-catch`.
- **Excepciones**: Lanzar `InvalidOperationException`, `ArgumentException`, etc., en caso de error de negocio.