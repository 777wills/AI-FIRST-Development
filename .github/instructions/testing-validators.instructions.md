---
name: 'Testing Validadores'
description: 'Pruebas de Validadores Fluent.'
applyTo: 'tests/**/Validators/**/*Tests.cs'
---
# Tests de Validadores
- Usar `[TestMethod]` con `[DataRow]` para comprobar varios inputs inválidos en un solo método (`[DataTestMethod]` está deprecado — MSTEST0044).
- Usar `TestValidate(instance)` de FluentValidation y `result.ShouldHaveValidationErrorFor(x => x.Campo)` / `result.ShouldNotHaveAnyValidationErrors()`.

## Tests de Validators Paginados
- Validar `PageNumber` < 1 falla, `PageSize` fuera de [1,100] falla.
- Validar campo de filtro no en whitelist falla.
- Validar operador no permitido para campo falla.
- Validar campo de orden no en whitelist falla.
- Validar request válido con y sin filtros pasa.
- **Referencia**: `tests/Olimpia.Tests/Validators/GetAllProductsValidatorTests.cs`.
