---
name: 'Domain Entities'
description: 'Entidades del dominio.'
applyTo: 'src/**/Entities/**/*.cs'
---
# Entidades de Dominio
- **Base**: Heredar de `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`).
- **Sealed**: Todas las entidades concretas son `sealed`.
- **Propiedades**: Inicializar strings con `= string.Empty`.
- **Constructores**: Obligatorio constructor vacío (Dapper) y constructor parametrizado explícito.