---
name: 'Stored Procedures y Vistas'
description: 'Acceso a SPs y Vistas.'
applyTo: 'src/**/*StoredProcedure*.cs,src/**/*View*.cs'
---
# Stored Procedures y Views
- Usar `IStoredProcedureRepository` para SPs. 
  - `ExecuteAsync` para no-results.
  - `QueryAsync<T>` para listas.
  - Pasar `DynamicParameters` para outputs.
- Usar `IViewRepository` para vistas (`QueryAsync<T>`, `QueryPagedAsync<T>`).