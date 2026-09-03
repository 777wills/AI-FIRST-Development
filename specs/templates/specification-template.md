---
id: "[ID del Work Item de Azure DevOps]"
title: "[Título corto descriptivo del feature]"
version: "1.0"
date_created: "[YYYY-MM-DD]"
last_updated: "[YYYY-MM-DD]"
source: "[URL al work item de Azure DevOps]"
status: "borrador | en_revisión | aprobada"
---

# Especificación: [Título del Feature]

<!-- TOC: §1-Historia §2-Resumen §3-Contexto §4-Contexto-Técnico §5-Requisitos-Funcionales §6-Requisitos-No-Funcionales §7-Criterios-de-Aceptación §8-Alcance §9-Modelo-de-Datos §10-Endpoints §11-Reglas-de-Negocio §12-Validaciones §13-Autorización §14-Dependencias §15-Riesgos §16-Preguntas-Abiertas -->

## 1. Historia de Usuario Original

> Como [rol], quiero [acción], para [beneficio].

*(Historia tal como viene de Azure DevOps Boards — no parafrasear)*

## 2. Resumen Ejecutivo

[Un párrafo. Qué problema resuelve, qué valor de negocio aporta, y cuál es el resultado esperado tras la implementación.]

## 3. Contexto

**Estado actual (As-Is):** [Cómo se resuelve hoy el problema y cuáles son sus limitaciones.]

**Estado deseado (To-Be):** [Qué podrán hacer los usuarios una vez implementado el feature.]

## 4. Contexto Técnico Descubierto

> Completado automáticamente por el Spec Builder con hallazgos del Codebase Explorer. Objetivo: que Plan Builder, Task Definer y Orchestrator hereden este contexto sin re-explorar el codebase.

### Hallazgos Domain

| Entidad | Archivo | Propiedades clave | Relación con el feature |
|---------|---------|-------------------|------------------------|
| [Nombre] | `src/Olimpia.Domain/Entities/[Nombre].cs` | [Props] | [Cómo se relaciona] |

| Interfaz de Repositorio | Archivo | Métodos custom |
|-------------------------|---------|----------------|
| `I[Nombre]Repository` | `src/Olimpia.Domain/Repositories/I[Nombre]Repository.cs` | [Métodos específicos] |

- **BaseEntity:** [Qué propiedades base aporta — Id, CreatedAt, UpdatedAt, etc.]
- **Convenciones detectadas:** [Patrones de herencia, naming, restricciones observadas]

### Hallazgos Application

| Tipo | Archivo de referencia | Patrón/Interfaz |
|------|----------------------|-----------------|
| Command | `src/Olimpia.Application/[Feature]/Commands/[Action]/[Action]Command.cs` | `ICommand<T>` |
| Handler | `src/Olimpia.Application/[Feature]/Commands/[Action]/[Action]Handler.cs` | `ICommandHandler<,>` |
| Validator | `src/Olimpia.Application/[Feature]/Commands/[Action]/[Action]Validator.cs` | `AbstractValidator<T>` |
| Query | `src/Olimpia.Application/[Feature]/Queries/[Action]/[Action]Query.cs` | `IQuery<T>` |
| QueryHandler | `src/Olimpia.Application/[Feature]/Queries/[Action]/[Action]Handler.cs` | `IQueryHandler<,>` |
| DTO | `src/Olimpia.Application/[Feature]/Queries/[Action]/[Action]Dto.cs` | `sealed record` |

- **Estructura de features:** [Cómo se organizan las carpetas por feature]
- **Contratos y DI:** [Interfaces en Contracts/, registro en DependencyInjection.cs]

### Hallazgos Infrastructure

| Repositorio | Archivo | Interfaz | Métodos custom |
|-------------|---------|----------|----------------|
| [Nombre]Repository | `src/Olimpia.Infrastructure/Persistence/Repositories/[Nombre]Repository.cs` | `I[Nombre]Repository` | [Métodos] |

- **DI y Scrutor:** [Auto-registro de repositorios, decorators de retry]
- **UnitOfWork:** [BeginTransactionAsync / CommitAsync / RollbackAsync]
- **HTTP Clients:** [Clients existentes y configuración de Polly si aplica]

### Hallazgos Api

| Controller | Archivo | Rutas existentes | Scopes de Auth |
|------------|---------|-----------------|----------------|
| [Nombre]Controller | `src/Olimpia.Api/Controllers/V1/[Nombre]Controller.cs` | [Rutas] | [Scopes] |

- **Middleware pipeline:** ExceptionMiddleware → RequestLogging → Audit → HTTPS → RateLimit → Auth → Controllers
- **Versioning:** Controllers en `V{N}/`, atributos `[ApiVersion]` y `[MapToApiVersion]`

### Hallazgos Tests

| Clase de test | Archivo | Tipo | Patrón observado |
|---------------|---------|------|-----------------|
| [Feature]HandlerTests | `tests/Olimpia.Tests/Handlers/[Feature]/` | Command/Query | [Patrón] |
| [Feature]ValidatorTests | `tests/Olimpia.Tests/Validators/` | Validator | [Patrón] |

- **Setup:** [Constructor vs TestInitialize, MockFactory, Fixtures]
- **Patterns:** [Naming `Metodo_Should_Result_When_Condition`, uso de DataRow, global::]

## 5. Requisitos Funcionales

> Formato EARS: `Cuando <evento>, el sistema deberá <respuesta>`. Agregar `While <estado>` cuando el requisito dependa de una condición previa.

- **RF-01** — [Prioridad: Crítica | Alta | Media | Baja]
  Cuando [trigger o acción del usuario], el sistema deberá [comportamiento esperado].

- **RF-02** — [Prioridad: Crítica | Alta | Media | Baja]
  Cuando [trigger], el sistema deberá [comportamiento].

- **RF-03** — [Prioridad: Media | Baja]
  While [estado del sistema], Cuando [trigger], el sistema deberá [comportamiento].

*(Agregar o quitar requisitos según la spec. Cada RF debe ser verificable con un test.)*

## 6. Requisitos No Funcionales

- **Rendimiento:** Tiempo de respuesta ≤ [N ms] bajo [N] usuarios concurrentes.
- **Seguridad:** [Autenticación requerida / Datos a proteger / Restricciones PII].
- **Disponibilidad:** [Uptime objetivo, comportamiento en fallos de dependencias externas].
- **Escalabilidad:** [Volumen esperado de datos/requests y estrategia].

*(Omitir los que no aplican. Agregar los específicos del feature.)*

## 7. Criterios de Aceptación

> Formato Gherkin (Given/Cuando/Then). Cada escenario es ejecutable como test de aceptación.
> Nombrar escenarios con verbos de negocio, no técnicos.

```gherkin
Scenario: [Nombre del escenario — happy path]
  Given [contexto inicial / estado del sistema]
  And [condición adicional si aplica]
  Cuando [acción del usuario o evento del sistema]
  Then [resultado observable esperado]
  And [resultado adicional observable si aplica]

Scenario: [Nombre del escenario — edge case o error]
  Given [contexto]
  Cuando [acción que provoca el caso borde o error]
  Then [comportamiento esperado del sistema ante el error]
```

*(Cubrir: happy path, validaciones fallidas, usuario no autorizado, recurso no encontrado, y cualquier edge case relevante de las reglas de negocio.)*

## 8. Alcance

**Incluido:**
- [Lo que SÍ cubre esta especificación]

**Excluido (futuros sprints):**
- [Lo que NO cubre — mejoras futuras a considerar]

## 9. Modelo de Datos

### Entidades Nuevas

| Entidad | Propiedad | Tipo C# | Tipo SQL | Restricciones | Descripción |
|---------|-----------|---------|----------|---------------|-------------|
| [Nombre] | [Prop] | [string/int/decimal] | [VARCHAR/INT] | [NOT NULL / UNIQUE] | [Descripción negocio] |

### Entidades Modificadas

| Entidad | Propiedad | Cambio | Razón |
|---------|-----------|--------|-------|
| [Nombre] | [Prop] | [Agregar / Modificar tipo / Eliminar] | [Por qué] |

*(Omitir tabla de modificadas si no hay cambios en entidades existentes.)*

## 10. Endpoints API

| Método | Ruta | Descripción | Auth | Request Body | Response Body | Códigos HTTP |
|--------|------|-------------|------|-------------|---------------|-------------|
| POST | `/api/v1/[recurso]` | [Descripción] | Sí | `[Action][Feature]Command` | `{ id: int }` | 200, 400, 409 |
| GET | `/api/v1/[recurso]/{id}` | [Descripción] | Sí | — | `[Feature]Dto` | 200, 404 |
| PUT | `/api/v1/[recurso]/{id}` | [Descripción] | Sí | `[Action][Feature]Command` | `{ affected: int }` | 200, 400, 404 |
| DELETE | `/api/v1/[recurso]/{id}` | [Descripción] | Sí | — | — | 204, 404 |

## 11. Reglas de Negocio

> Las reglas de negocio son invariantes que el sistema SIEMPRE debe cumplir, independientemente del endpoint o flujo.

- **RN-01:** [Descripción de la regla. Qué debe ser siempre verdad.]
- **RN-02:** [Descripción de la regla.]

## 12. Validaciones

| Campo | Regla | Mensaje de Error al Usuario |
|-------|-------|-----------------------------|
| [Campo] | [Obligatorio / Longitud máx. N / Formato regex / Valor > 0] | [Mensaje claro en español] |

## 13. Requisitos de Autorización

| Endpoint | Scope / Política | Notas |
|----------|-----------------|-------|
| `GET /api/v1/[recurso]` | `[feature].read` | Todos los usuarios autenticados |
| `POST /api/v1/[recurso]` | `[feature].write` | Solo roles con permiso de escritura |
| `PUT /api/v1/[recurso]/{id}` | `[feature].write` | — |
| `DELETE /api/v1/[recurso]/{id}` | `[feature].write` | — |

*(Usar `[AllowAnonymous]` solo si el endpoint es genuinamente público. Documentar la razón.)*

## 14. Dependencias

**Internas:** [Feature X] — [Cómo se relaciona o impacta]

**Externas:** [API/Servicio de terceros] — [Propósito y contrato esperado]

**Impacto en features existentes:** [Feature Y] — [Qué cambia o se ve afectado]

*(Omitir secciones que no apliquen.)*

## 15. Riesgos

| Riesgo | Impacto | Probabilidad | Mitigación |
|--------|---------|-------------|------------|
| [Riesgo técnico o de negocio] | Alto / Medio / Bajo | Alta / Media / Baja | [Acción concreta] |

## 16. Preguntas Abiertas

> Preguntas sin resolver al momento de crear la spec. Cada una debe resolverse antes de la aprobación final.

1. [Pregunta — Responsable: Developer / PO — Fecha límite: YYYY-MM-DD]

## Aprobación

- [ ] **Developer:** [Nombre] — Fecha: [YYYY-MM-DD]
- [ ] **Tech Lead:** [Nombre] — Fecha: [YYYY-MM-DD]
