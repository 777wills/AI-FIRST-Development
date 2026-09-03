---
id: "[ID del Work Item de Azure DevOps]"
title: "Plan de Implementación: [Título del Feature]"
version: "1.0"
date_created: "[YYYY-MM-DD]"
last_updated: "[YYYY-MM-DD]"
spec_ref: "specs/active/{ID}-{feature-name}/specification.md"
status: "borrador | en_revisión | aprobado"
---

# Plan de Implementación: [Título del Feature]

<!-- TOC: §Resumen §Capas-Afectadas §Contexto-Técnico §Fases §Contratos-API §Verificación §Riesgos -->

## Resumen Ejecutivo

[TL;DR — Qué se implementa, decisión arquitectónica principal, y estrategia elegida. Máximo 3 oraciones.]

**Spec de referencia:** `specs/active/{ID}-{feature-name}/specification.md`

## Capas Afectadas

| Capa | Impacto | Archivos principales |
|------|---------|---------------------|
| Domain | Nuevo / Modificado / Sin cambios | [Entidades e interfaces a crear/modificar] |
| Application | Nuevo / Modificado / Sin cambios | [Commands, Queries, Handlers, Validators] |
| Infrastructure | Nuevo / Modificado / Sin cambios | [Repositorios, HTTP Clients, DI] |
| Api | Nuevo / Modificado / Sin cambios | [Controllers, endpoints] |
| Database | Nuevo / Modificado / Sin cambios | [Scripts SQL, tablas, SPs, vistas] |
| Tests | Nuevo | [Tests unitarios por capa] |

## Decisiones de Diseño

> Decisiones no-obvias que un desarrollador o agente podría cuestionar. Incluir la alternativa descartada y el motivo.

1. **[Decisión tomada]** — Razón: [Por qué esta opción y no la alternativa X].
2. **[Decisión tomada]** — Razón: [Por qué].

## Contexto Técnico Acumulado

> Fuente única de verdad para Task Definer y Orchestrator. Contiene el contexto heredado de la especificación más hallazgos adicionales del Plan Builder. Los agentes NO necesitan consultar la especificación.

### Heredado de la Especificación

#### Domain
[Copiar íntegramente la sección "Hallazgos Domain" de la spec — entidades, interfaces, propiedades, convenciones]

#### Application
[Copiar íntegramente "Hallazgos Application" — archivos de referencia, patrones CQRS, estructura]

#### Infrastructure
[Copiar íntegramente "Hallazgos Infrastructure" — repositorios, DI, UnitOfWork, HTTP Clients]

#### Api
[Copiar íntegramente "Hallazgos Api" — controllers, pipeline, versioning, auth]

#### Tests
[Copiar íntegramente "Hallazgos Tests" — patrones de test, naming, fixtures, mocks]

### Hallazgos Adicionales (Plan Builder)

> Solo lo que NO está en la especificación. Resultado de exploración adicional del codebase.

- **DI:** [Cómo se registran repositorios/servicios — auto-discovery con Scrutor en `DependencyInjection.cs` o registro manual]
- **Decorators:** [Si aplican retry decorators u otros wrappers de Scrutor]
- **Test de referencia para commands:** `tests/Olimpia.Tests/Handlers/[Feature]/`
- **Test de referencia para queries:** `tests/Olimpia.Tests/Handlers/[Feature]/`
- **Estado del codebase:** [Observaciones relevantes: archivos que necesitarán modificación además de creación, conflictos potenciales]

## Fases de Implementación

### Fase 1: Domain

**Objetivo:** Definir entidades y contratos del dominio. Sin dependencias externas.

| Acción | Archivo | Referencia |
|--------|---------|------------|
| Crear | `src/Olimpia.Domain/Entities/[Nombre].cs` | `src/Olimpia.Domain/Entities/Product.cs` |
| Crear | `src/Olimpia.Domain/Repositories/I[Nombre]Repository.cs` | `src/Olimpia.Domain/Repositories/IProductRepository.cs` |

**Checkpoint:** `dotnet build src/Olimpia.Domain`

### Fase 2: Application Scaffolding

**Objetivo:** Crear archivos declarativos (records, DTOs) antes del ciclo TDD. El TDD Implementer usará estos como base para implementar Handlers y Validators.

| Acción | Archivo |
|--------|---------|
| Crear | `src/Olimpia.Application/[Feature]/Commands/[Action][Feature]/[Action][Feature]Command.cs` |
| Crear | `src/Olimpia.Application/[Feature]/Queries/[Action][Feature]/[Action][Feature]Query.cs` |
| Crear | `src/Olimpia.Application/[Feature]/Queries/[Action][Feature]/[Action][Feature]Dto.cs` |

**Checkpoint:** `dotnet build src/Olimpia.Application`

### Fase 3: TDD — Handlers, Validators y Tests

**Objetivo:** Ciclo completo Red → Green → Refactor para lógica de negocio. Ejecutada por el **TDD Implementer**.
**Depende de:** Fase 1 y Fase 2.

| Acción | Archivo |
|--------|---------|
| Crear (test primero) | `tests/Olimpia.Tests/Handlers/[Feature]/[Action][Feature]HandlerTests.cs` |
| Crear (test primero) | `tests/Olimpia.Tests/Validators/[Action][Feature]ValidatorTests.cs` |
| Crear (tras test rojo) | `src/Olimpia.Application/[Feature]/Commands/[Action][Feature]/[Action][Feature]Handler.cs` |
| Crear (tras test rojo) | `src/Olimpia.Application/[Feature]/Commands/[Action][Feature]/[Action][Feature]Validator.cs` |

**Checkpoint:** `dotnet test` — todos los tests PASAN (Green).

### Fase 4: Infrastructure + Database

**Objetivo:** Implementar persistencia y scripts SQL.
**Depende de:** Fase 1.
**Puede ejecutarse en paralelo con:** Fase 3.

| Acción | Archivo | Referencia |
|--------|---------|------------|
| Crear | `src/Olimpia.Infrastructure/Persistence/Repositories/[Nombre]Repository.cs` | `ProductRepository.cs` |
| Crear | `scripts/[Nombre]s.sql` | Convenciones en `database.instructions.md` |

**Checkpoint:** `dotnet build src/Olimpia.Infrastructure`

### Fase 5: Api

**Objetivo:** Exponer endpoints REST.
**Depende de:** Fase 3 y Fase 4.

| Acción | Archivo | Referencia |
|--------|---------|------------|
| Crear | `src/Olimpia.Api/Controllers/V1/[Feature]Controller.cs` | `ProductController.cs` |

**Checkpoint:** `dotnet build` (solución completa) + `dotnet test`

### Fase 6: Code Review y Cobertura

**Objetivo:** Calidad y cobertura ≥95% en archivos nuevos.
**Depende de:** Fase 5.

1. Invocar **Code Reviewer** — corregir todos los issues críticos.
2. `dotnet test --collect:"XPlat Code Coverage" --settings tests/Olimpia.Tests/coverage.runsettings`
3. Si cobertura < 95%, invocar **TDD Implementer** con la lista exacta de archivos y métodos sin cubrir.
4. Repetir hasta ≥95% (máximo 3 iteraciones).

**Checkpoint:** Revisión aprobada + cobertura ≥95% en archivos nuevos.

### Fase 6.5: Verificación de Cumplimiento de Spec

**Objetivo:** Verificar que cada requisito funcional, criterio de aceptación, regla de negocio, validación, contrato de endpoint y restricción del modelo de datos de la especificación fue implementado correctamente. Detectar gold-plating.
**Depende de:** Fase 6.

1. Invocar **Spec Compliance Verifier** — cross-referencia implementación contra spec original.
2. Si veredicto NO CUMPLE, corregir gaps con el sub-agente de la capa afectada (máximo 2 iteraciones).
3. Guardar reporte en `specs/active/{ID}-{feature-name}/compliance-report.md`.

**Checkpoint:** Veredicto CUMPLE o CUMPLE CON ADVERTENCIAS.

### Fase 7: Documentación

**Objetivo:** Actualizar docs si hubo cambios en endpoints, arquitectura o paquetes.
**Depende de:** Fase 6.

Invocar **Doc Updater**. Si no hay cambios relevantes, reportar "Sin cambios necesarios".

**Checkpoint:** Documentación actualizada en disco.

## Contratos API

| Método | Ruta | Request Body | Response Body | Códigos HTTP esperados |
|--------|------|-------------|---------------|----------------------|
| POST | `/api/v1/[recurso]` | `[Action][Feature]Command` | `{ id: int }` | 200, 400, 409 |
| GET | `/api/v1/[recurso]/{id}` | — | `[Feature]Dto` | 200, 404 |

## Verificación Final

- [ ] `dotnet build` — sin warnings.
- [ ] `dotnet test` — todos los tests pasan.
- [ ] `dotnet test --collect:"XPlat Code Coverage"` — cobertura ≥95% en archivos nuevos.
- [ ] Swagger (`/swagger`) muestra los nuevos endpoints.
- [ ] Code Reviewer: veredicto APROBADO.
- [ ] Spec Compliance Verifier: veredicto CUMPLE o CUMPLE CON ADVERTENCIAS.

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|--------|-------------|------------|
| [Riesgo técnico o de dependencia] | Alta / Media / Baja | [Acción concreta de mitigación] |

## Aprobación

- [ ] **Developer:** [Nombre] — Fecha: [YYYY-MM-DD]
