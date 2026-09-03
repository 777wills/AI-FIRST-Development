---
name: 'Convenciones Base de Datos'
description: 'Convenciones SQL.'
applyTo: '**/*.sql'
---
# Convenciones SQL
- Iniciar scripts con `USE [APIBase]; GO`.
- Tablas: PascalCase plural (Products).
- Columnas: PascalCase.
- Idempotencia: usar `IF NOT EXISTS` o `IF OBJECT_ID() IS NULL`.
- Tipos: `INT IDENTITY(1,1)`, `NVARCHAR(N)`, `DECIMAL(18,2)`, `DATETIME2(7)`, `BIT`.