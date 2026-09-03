---
name: 'Domain Interfaces'
description: 'Interfaces de repositorios y contratos.'
applyTo: 'src/Olimpia.Domain/Repositories/I*.cs'
---
# Interfaces del Dominio
- **Repositorios**: Interfaz hereda de `IGenericRepository<T>`.
- Declarar solo firmas de métodos específicos del dominio.
- El dominio NO tiene dependencias externas.
- `IGenericRepository<T>` expone `GetPagedAsync(pageNumber, pageSize, filters, sortFields)` para paginación genérica.
- Los tipos `FilterCriteria`, `SortCriteria` y `FilterOperator` viven en `Olimpia.Domain.Common`.
