---
name: 'UnitOfWork (Transacciones)'
description: 'Reglas para UnitOfWork y transacciones.'
applyTo: 'src/**/Commands/**/*Handler.cs,src/**/UnitOfWork.cs'
---
# UnitOfWork
- Inyectar `IUnitOfWork` en Command Handlers que modifican datos.
- Patrón obligatorio:
  ```csharp
  await _unitOfWork.BeginTransactionAsync();
  try {
      // Modificaciones en repositorios
      await _unitOfWork.CommitAsync();
  } catch {
      await _unitOfWork.RollbackAsync();
      throw;
  }
  ```