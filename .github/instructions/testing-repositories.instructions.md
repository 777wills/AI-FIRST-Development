---
name: 'Testing Repositorios'
description: 'Pruebas de Repositorios.'
applyTo: 'tests/**/Repositories/**/*.cs'
---
# Tests de Repositorios
- Inicializar mocks en **constructor** con campos `readonly` (best practice MSTest .NET 10).
- Mockear `QueryFactory` y transacciones.
- Usar `.Should().NotBeNull()` y validaciones de FluentAssertions.
- Cuando `UnitOfWork` es `sealed`, complementar con tests de SQL Query Shape e Interface Contract.
