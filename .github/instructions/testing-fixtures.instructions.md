---
name: 'Testing Fixtures'
description: 'Creación de Fixtures.'
applyTo: 'tests/**/Fixtures/**/*.cs'
---
# Fixtures
- Crear clases estáticas con datos válidos por defecto. Nombre estándar de factory methods: `CreateValid(...)` (ej. `ProductFixture.CreateValid()`).
- Factory de mocks comunes (ej. `MockFactory.CreateUnitOfWorkMock()`) con setup de `BeginTransactionAsync`, `CommitAsync` y `RollbackAsync` para evitar duplicar configuraciones de Moq en cada clase.
