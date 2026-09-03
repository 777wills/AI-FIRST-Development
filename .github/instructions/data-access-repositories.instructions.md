---
name: 'Repositorios (Acceso a Datos)'
description: 'Implementación del patrón Repository.'
applyTo: 'src/Olimpia.Infrastructure/Persistence/Repositories/*.cs'
---
# GenericRepository<T>
- **Base**: Repositorios concretos heredan de `GenericRepository<T>` e implementan `I<Entity>Repository`.
- **Implementación**: Solo agregar métodos de dominio específicos (no reimplementar CRUD).
- **Auto-registro**: Se registran automáticamente en DI. No registrarlos a mano.
- Siempre inyectar la interfaz (ej. `IProductRepository`), nunca la clase concreta.
- **Paginación**: `GetPagedAsync` está disponible en `GenericRepository<T>`. Ejecuta COUNT y DATA en queries separadas con SqlKata. Los filtros se traducen vía `ApplyFilter` (método privado). Para `Contains`, se escapan `%` y `_` antes de `WhereLike`.
- **Decorador Retry**: `GenericRepositoryRetryDecorator<T>` aplica retry solo a **lecturas** (`GetByIdAsync`, `GetAllAsync`, `GetPagedAsync`). Las escrituras (`AddAsync`, `UpdateAsync`, `DeleteAsync`) no se reintentan (no son idempotentes).
