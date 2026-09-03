---
name: 'SqlKata (Acceso a Datos)'
description: 'Reglas para consultas con SqlKata.'
applyTo: 'src/**/Repositories/**/*.cs'
---
# SqlKata API Fluida
- **Prohibido SQL Crudo**: Nunca usar `Db.Statement("SELECT...")`.
- Utilizar la API fluida: `Db.Query("Products").Where("Id", id)...`
- **Transacciones**: Pasar SIEMPRE la transacción en métodos Dapper/SqlKata: `transaction: UnitOfWork.DbTransaction`.
- **Paginación**: Usar `.Offset((pageNumber - 1) * pageSize).Limit(pageSize)` para datos y `.CountAsync<int>()` para total — en queries separadas.
- **Filtros dinámicos**: Traducir `FilterOperator` a SqlKata: `Eq` → `.Where()`, `Contains` → `.WhereLike()` (escapar `%` y `_` en el valor), `Gt/Gte/Lt/Lte` → `.Where(field, ">", value)`, etc.
- **Ordenamiento dinámico**: `.OrderBy(field)` o `.OrderByDesc(field)` según `SortCriteria.Descending`.
