---
name: 'CQRS Validators'
description: 'Validadores FluentValidation para Commands y Queries.'
applyTo: 'src/**/Validators/**/*.cs,src/**/*Validator.cs'
---
# CQRS Validators
- **Framework**: `FluentValidation`.
- **Ubicación**: Misma carpeta o adyacente al Command/Query.
- **Implementación**: Heredar de `AbstractValidator<T>`.
- **Regla**: Un validador por Command/Query. Se invocan automáticamente.
- **Mensajes**: En español (destinados a usuarios finales).

## Validators de Queries Paginadas
- Validar `PageNumber >= 1` y `PageSize` entre 1 y 100.
- Definir whitelist de campos filtrables como `Dictionary<string, IReadOnlyList<FilterOperator>>` (case-insensitive).
- Definir whitelist de campos ordenables como `HashSet<string>` (case-insensitive).
- Validar que cada filtro tenga campo y operador dentro de la whitelist.
- Validar tipo de dato del valor del filtro (`decimal.TryParse`, `DateTime.TryParse`).
- **Referencia**: `src/Olimpia.Application/Products/Queries/GetAllProducts/GetAllProductsValidator.cs`.
