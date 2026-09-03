# 🤖 Propuesta: Flujo de Desarrollo AI-First con GitHub Copilot Agents

## Tabla de Contenidos

- [1. Resumen Ejecutivo](#1-resumen-ejecutivo)
- [2. Problema que Resuelve](#2-problema-que-resuelve)
- [3. Arquitectura de Agentes](#3-arquitectura-de-agentes)
  - [3.1 Diagrama General del Flujo](#31-diagrama-general-del-flujo)
  - [3.2 Agentes Principales (User-Facing)](#32-agentes-principales-user-facing)
  - [3.3 Sub-agentes Especializados](#33-sub-agentes-especializados)
  - [3.4 Modelo de Contexto Aislado](#34-modelo-de-contexto-aislado)
- [4. Flujo Detallado Paso a Paso](#4-flujo-detallado-paso-a-paso)
  - [Fase 1 — Especificación](#fase-1--especificación-spec-builder)
  - [Fase 2 — Plan de Implementación](#fase-2--plan-de-implementación-plan-builder)
  - [Fase 3 — Definición de Tareas](#fase-3--definición-de-tareas-task-definer)
  - [Fase 4 — Implementación Orquestada](#fase-4--implementación-orquestada-orchestrator)
  - [Fase 5 — Pull Request](#fase-5--pull-request-pr-builder)
- [5. Estructura de Archivos](#5-estructura-de-archivos)
- [6. Componentes del Framework](#6-componentes-del-framework)
  - [6.1 Instructions (Instrucciones)](#61-instructions-instrucciones)
  - [6.2 Skills (Habilidades)](#62-skills-habilidades)
  - [6.3 Templates (Plantillas)](#63-templates-plantillas)
  - [6.4 Hooks (Automatización)](#64-hooks-automatización)
  - [6.5 Prompts (Comandos Rápidos)](#65-prompts-comandos-rápidos)
- [7. Handoffs — Transiciones entre Agentes](#7-handoffs--transiciones-entre-agentes)
- [8. Hooks — Eventos y Automatización](#8-hooks--eventos-y-automatización)
- [9. Ciclo TDD (Red → Green → Refactor)](#9-ciclo-tdd-red--green--refactor)
- [10. Checkpoints Verificables](#10-checkpoints-verificables)
- [11. Mecanismos de Seguridad](#11-mecanismos-de-seguridad)
- [12. Ejemplo 1 — CRUD de Categorías](#12-ejemplo-1--crud-de-categorías)
- [13. Ejemplo 2 — Query de Búsqueda con Filtros](#13-ejemplo-2--query-de-búsqueda-con-filtros)
- [14. Ejemplo 3 — Feature con Integración HTTP Externa](#14-ejemplo-3--feature-con-integración-http-externa)
- [15. Preguntas Frecuentes](#15-preguntas-frecuentes)

---

## 1. Resumen Ejecutivo

Esta propuesta implementa un flujo de desarrollo **Spec-Driven Development (SDD)** asistido por **GitHub Copilot Agents en VS Code** para el proyecto **Olimpia API** (.NET 10, Clean Architecture, CQRS).

El flujo transforma una historia de usuario de Azure DevOps en código productivo mediante una cadena de **4 agentes principales** que operan secuencialmente con aprobación explícita del desarrollador en cada paso, y un **orquestador** que delega a **sub-agentes especializados** — cada uno en su propia ventana de contexto para evitar desbordamiento.

**Principios clave:**
- 🧠 **Contexto aislado**: Cada sub-agente corre en su propia ventana para evitar "ruido".
- ✋ **Aprobación explícita**: El desarrollador aprueba antes de avanzar a la siguiente fase.
- 🔴🟢🔵 **TDD estricto**: Tests primero (Red), implementación mínima (Green), refactor (Refactor) consolidados en un único implementador TDD.
- 🔍 **Verificación continua**: Build y tests se ejecutan tras cada cambio.
- ❓ **Sin supuestos**: Los agentes preguntan ante cualquier ambigüedad.

---

## 2. Problema que Resuelve

| Problema | Solución |
|----------|----------|
| La ventana de contexto de un solo agente se desborda en tareas complejas | Arquitectura coordinador → sub-agentes con contexto aislado. Uso de un `Codebase Explorer` unificado. |
| La IA asume decisiones sin consultar al developer | Fases de clarificación obligatorias + `vscode/askQuestions` |
| Código se implementa sin especificación clara | Flujo Spec → Plan → Tasks antes de tocar código |
| Tests se escriben después del código | TDD obligatorio integrado en un único agente resolutivo |
| Errores se acumulan hasta el final | Verificación continua: build y test tras cada sub-agente |
| No hay trazabilidad del proceso | Artefactos versionados en `specs/active/{ID}-{feature}/` (specification, plan, tasks) |
| Errores cross-layer se ignoran o corrigen en la capa equivocada | Flujo cíclico: sub-agentes reportan → orchestrator enruta al agente correcto |

---

## 3. Arquitectura de Agentes

### 3.1 Diagrama General del Flujo

```
                    ┌─────────────────────────────────────────────┐
                    │           DEVELOPER (tú)                    │
                    │  Pega la Historia de Usuario de Azure DevOps│
                    └──────────────┬──────────────────────────────┘
                                   │
                                   ▼
                    ┌──────────────────────────────┐
                    │     📋 SPEC BUILDER          │  Fase 1
                    │  Construye especificación    │
                    │  interactiva con el dev      │
                    │                              │
                    │  🔍 Codebase Explorer        │
                    │     (Descubrimiento unificado)│
                    └──────────┬───────────────────┘
                               │ ✅ Aprobación del dev
                               │ 📌 Nueva sesión: /plan-from-spec {ID}
                               ▼
                    ┌──────────────────────────────┐
                    │     📐 PLAN BUILDER          │  Fase 2
                    │  Crea plan de implementación │
                    │  con fases TDD               │
                    │                              │
                    │  🔍 Codebase Explorer        │
                    │     (solo gaps)              │
                    └──────────┬───────────────────┘
                               │ ✅ Aprobación del dev
                               │ 📌 Nueva sesión: /tasks-from-plan {ID}
                               ▼
                    ┌──────────────────────────────┐
                    │     📝 TASK DEFINER          │  Fase 3
                    │  Define tareas granulares    │
                    │  con dependencias T-XXX      │
                    │                              │
                    │  🔍 Codebase Explorer        │
                    │     (verificación mínima)    │
                    └──────────┬───────────────────┘
                               │ ✅ Aprobación del dev
                               │ 📌 Nueva sesión: /implement-tasks {ID}
                               ▼
                    ┌──────────────────────────────┐
                    │     🎯 ORCHESTRATOR          │  Fase 4
                    │  Lee tareas y delega a       │
                    │  sub-agentes especializados  │
                    │                              │
                    │  ┌─────────────────────────┐ │
                    │  │  11 SUB-AGENTES        │ │
                    │  │  (contexto aislado c/u) │ │
                    │  └─────────────────────────┘ │
                    └──────────┬───────────────────┘
                               │ ✅ Aprobación del dev
                               │ 📌 Nueva sesión: /create-pr {ID}
                               ▼
                    ┌──────────────────────────────┐
                    │     🔀 PR BUILDER            │  Fase 5
                    │  Redacta PR (título, desc,   │
                    │  work items) y lo muestra    │
                    │  al dev ANTES de publicar    │
                    └──────────┬───────────────────┘
                               │ ✅ Aprobación del dev
                               │ 🚀 Publica a Azure DevOps
                               ▼
                    ┌──────────────────────────────┐
                    │     Azure DevOps PR          │
                    │     (Pull Request creado)    │
                    └──────────────────────────────┘
```

### 3.2 Agentes Principales (User-Facing)

Estos son los 5 agentes que el desarrollador invoca directamente desde el chat de VS Code:

| # | Agente | Icono | Propósito | Modelo |
|---|--------|-------|-----------|--------|
| 1 | **Spec Builder** | 📋 | Construye especificación técnica a partir de una HU | Claude Opus 4.6 |
| 2 | **Plan Builder** | 📐 | Crea plan de implementación con fases TDD | Claude Opus 4.6 |
| 3 | **Task Definer** | 📝 | Define tareas granulares con dependencias | Claude Sonnet 4.6 |
| 4 | **Orchestrator** | 🎯 | Orquesta la implementación delegando a sub-agentes | Claude Sonnet 4.6 |
| 5 | **PR Builder** | 🔀 | Redacta y publica el Pull Request en Azure DevOps (con aprobación previa) | Claude Sonnet 4.6 |

### 3.3 Sub-agentes Especializados

Estos **NO aparecen en el dropdown** del chat (son `user-invocable: false`). El **Codebase Explorer** es invocado por Spec Builder, Plan Builder y Task Definer. Los demás solo los invoca el Orchestrator:

| Sub-agente | Propósito | Tools | Modelo |
|------------|-----------|-------|--------|
| **Codebase Explorer** | Exploración read-only unificada en todo el proyecto | search, read | Claude Haiku 4.5 |
| **TDD Implementer** | Ciclo Red→Green→Refactor para Handlers y Validators | search, read, edit, execute | Claude Sonnet 4.6 |
| **Domain Implementer** | Entidades e interfaces de repositorio | search, read, edit | Claude Sonnet 4.6 |
| **Application Implementer** | Scaffolding CQRS: Command/Query records, DTOs, contratos | search, read, edit | Claude Sonnet 4.6 |
| **Infrastructure Implementer** | Repositorios, DI, decorators | search, read, edit | Claude Sonnet 4.6 |
| **API Implementer** | Controllers REST | search, read, edit | Claude Sonnet 4.6 |
| **SQL Server Implementer** | Scripts SQL: tablas, SPs, vistas, índices | search, read, edit, execute | Claude Sonnet 4.6 |
| **Code Reviewer** | Revisión de código (solo lectura) | search, read | Claude Opus 4.6 |
| **Spec Compliance Verifier** | Verifica alineación spec↔código, detecta gold-plating | search, read | Claude Sonnet 4.6 |
| **Doc Updater** | Actualización de documentación | search, read, edit | Claude Sonnet 4.6 |
| **Coverage Analyzer** | Analiza cobertura de código ≥95% | search, read, execute | Claude Haiku 4.5 |

> **Prohibición:** El Orchestrator **NO debe invocar al Codebase Explorer**. Sus sub-agentes de implementación tienen tools `search` y `read` incorporados para explorar por su cuenta. El contexto técnico necesario ya está documentado en las secciones de spec, plan y tasks.

### 3.4 Modelo de Contexto Aislado

```
┌───────────────────── Ventana Principal ─────────────────────┐
│                                                              │
│  🎯 ORCHESTRATOR                                             │
│  • Lee la lista de tareas                                    │
│  • Decide qué sub-agente invocar                             │
│  • Recibe solo un RESUMEN del resultado                      │
│  • Su contexto NO se llena con el código generado            │
│                                                              │
│  ┌──── Ventana 1 ────┐  ┌──── Ventana 2 ────┐               │
│  │ 🔴 TDD Impl       │  │ 🏗️ Domain Impl    │               │
│  │ Ejecuta ciclo TDD │  │ Solo ve: entidades │               │
│  │ en un solo hilo   │  │ Su contexto es     │               │
│  │ INDEPENDIENTE     │  │ INDEPENDIENTE      │               │
│  └───────────────────┘  └────────────────────┘               │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**¿Por qué esto importa?** Si un solo agente hiciera todo, su ventana de contexto (~128K-200K tokens) se llenaría rápidamente con el código fuente, tests, y toda la conversación previa. Con sub-agentes aislados:

1. El **Orchestrator** solo mantiene la lista de tareas y resúmenes
2. Cada **sub-agente** recibe únicamente el contexto relevante a su tarea
3. Al terminar, el sub-agente devuelve un **resumen accionable**, no todo el código generado

### 3.5 Exploración Progresiva y Caché de Contexto

El **Codebase Explorer** usa un modelo de **exploración progresiva**: cada agente hereda los hallazgos del anterior y solo explora lo incremental. Esto evita re-explorar el codebase completo en cada fase.

```
┌──────────────────────────────────────────────────────────────┐
│              EXPLORACIÓN PROGRESIVA                          │
│                                                              │
│  📋 SPEC BUILDER                                             │
│  ├─ Invoca al Codebase Explorer para barrido general.        │
│  └─ Guarda hallazgos en: spec → "Contexto Técnico           │
│     Descubierto" (sub-secciones por capa)                    │
│          │                                                   │
│          ▼ hereda hallazgos                                  │
│  📐 PLAN BUILDER                                             │
│  ├─ COPIA íntegro "Contexto Técnico Descubierto" de la spec    │
│  ├─ Invoca al Codebase Explorer solo para gaps (ej. DI)      │
│  └─ Guarda TODO en: plan → "Contexto Técnico Acumulado"      │
│     (spec heredado + hallazgos nuevos del Plan Builder)     │
│          │                                                   │
│          ▼ contexto auto-contenido del plan                  │
│  📝 TASK DEFINER                                             │
│  ├─ COPIA íntegro "Contexto Técnico Acumulado" del plan       │
│  ├─ Invoca al Codebase Explorer (verificación mínima)        │
│  │   Solo: ¿existen archivos? ¿hay conflictos?              │
│  └─ Guarda TODO en: tasks → "Contexto Técnico Acumulado"    │
│     (plan heredado + verificación del Task Definer)         │
│          │                                                   │
│          ▼ contexto auto-contenido de las tareas             │
│  🎯 ORCHESTRATOR                                             │
│  ├─ LEE solo "Contexto Técnico Acumulado" de tasks           │
│  ├─ NO consulta spec ni plan para contexto técnico          │
│  ├─ NO invoca al explorador                                  │
│  └─ Sub-agentes tienen search + read propios                 │
│          │                                                   │
│          ▼ tras implementación (Paso 5.5)                    │
│  🔍 SPEC COMPLIANCE VERIFIER                                 │
│  ├─ ÚNICO agente que vuelve a la spec original               │
│  ├─ Cross-referencia spec ↔ código implementado              │
│  └─ Cierra el loop de trazabilidad                           │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## 4. Flujo Detallado Paso a Paso

### Fase 1 — Especificación (Spec Builder)

**Objetivo**: Transformar una historia de usuario en una especificación técnica completa.

```
Developer                         Spec Builder                  Codebase Explorer
    │                                  │                               │
    │  "Tengo esta HU: Como admin      │                               │
    │   quiero crear categorías..."    │                               │
    ├─────────────────────────────────►│                               │
    │                                  │  Invoca Codebase Explorer     │
    │                                  │  pidiendo buscar patrones     │
    │                                  │  existentes de CQRS, etc.     │
    │                                  ├──────────────────────────────►│
    │                                  │                               │  Codebase Explorer realiza
    │                                  │                               │  búsquedas dirigidas
    │                                  │◄──────────────────────────────┤
    │                                  │  Guarda en spec sección       │
    │                                  │  "Contexto Técnico            │
    │                                  │   Descubierto" (por capa)     │
    │                                  │                               │
    │  "¿La categoría tiene relación   │                               │
    │   padre-hijo (jerárquica)?"      │                               │
    │◄─────────────────────────────────┤                               │
    │                                  │                               │
    │  "No, es plana, solo nombre      │                               │
    │   y descripción"                 │                               │
    ├─────────────────────────────────►│                               │
    │                                  │                               │
    │                                  │  CREA archivo en              │
    │                                  │  specs/active/{ID}-{feature}/ │
    │                                  │  (status:borrador)            │
    │                                  │  CON hallazgos embebidos      │
    │  📄 "Spec creada en disco.       │                               │
    │  Puedes abrirla y editarla."     │                               │
    │◄─────────────────────────────────┤                               │
    │                                  │                               │
    │  (Developer abre, revisa,        │                               │
    │   puede editar directamente)     │                               │
    │  "Aprobado ✅"                    │                               │
    ├─────────────────────────────────►│                               │
    │                                  │  Actualiza status:aprobada    │
    │                                  │  Recomienda nueva sesión:     │
    │   📌 Siguiente paso:             │  /plan-from-spec {ID}         │
    │                                  │                               │
```

### Fase 2 — Plan de Implementación (Plan Builder)

**Objetivo**: Crear un plan detallado con fases TDD, archivos a crear/modificar y checkpoints.

**Se activa**: Invocando `/plan-from-spec {ID}` en una **nueva sesión de chat** (recomendado para preservar contexto). También puede usarse el handoff backward desde Task Definer si el plan necesita correcciones.

### Fase 3 — Definición de Tareas (Task Definer)

**Objetivo**: Descomponer el plan en tareas atómicas ejecutables con dependencias.

**Se activa**: Invocando `/tasks-from-plan {ID}` en una **nueva sesión** (recomendado).

### Fase 4 — Implementación Orquestada (Orchestrator)

**Objetivo**: Ejecutar las tareas delegando a sub-agentes especializados con TDD iterativo.

**Se activa**: Invocando `/implement-tasks {ID}` en una **NUEVA SESIÓN** (FUERTEMENTE RECOMENDADO — la implementación consume mucho contexto).

Esta es la fase más compleja. El Orchestrator lee la lista de tareas y para cada una:

```
Orchestrator                                              Sub-agentes
    │                                                          │
    │  Lee specs/active/{ID}-*/tasks.md (status:aprobadas)     │
    │  Lee secciones "Contexto Técnico" de tasks               │
    │                                                          │
    │  === PASO 1: DOMAIN ===                                  │
    │  🚀 Invoca Domain Implementer                            │
    ├─────────────────────────────────────────────────────────►│  Domain Implementer
    │◄─────────────────────────────────────────────────────────┤  ✅ Category.cs creado
    │                                                          │
    │  ✅ CHECKPOINT: dotnet build ── pasa                     │
    │                                                          │
    │  === PASO 1.5: APP SCAFFOLDING ===                       │
    │  🚀 Invoca Application Implementer                       │
    │  Contexto: "Crear records y DTOs para Category"          │
    ├─────────────────────────────────────────────────────────►│  Application Implementer
    │◄─────────────────────────────────────────────────────────┤  ✅ Command, Query, DTO creados
    │                                                          │
    │  ✅ CHECKPOINT: dotnet build ── pasa                     │
    │                                                          │
    │  === PASO 2: TDD (HANDLERS & VALIDATORS) ===             │
    │  🚀 Invoca TDD Implementer                               │
    │  Contexto: "TDD para CreateCategoryHandler"              │
    ├─────────────────────────────────────────────────────────►│  TDD Implementer
    │                                                          │  - Escribe tests (rojo)
    │                                                          │  - Escribe handler (verde)
    │                                                          │  - Refactoriza
    │◄─────────────────────────────────────────────────────────┤  ✅ Handlers, validators y tests
    │                                                          │
    │  ✅ CHECKPOINT: dotnet test ── tests pasan               │
    │                                                          │
    │  === PASO 3: INFRASTRUCTURE ===                          │
    │  🚀 Invoca Infrastructure Implementer                    │
    ├─────────────────────────────────────────────────────────►│  Infrastructure Implementer
    │◄─────────────────────────────────────────────────────────┤  ✅ CategoryRepository + DI
    │                                                          │
    │  === PASO 3.1: DATABASE (paralelo con Infra) ===         │
    │  🚀 Invoca SQL Server Implementer                        │
    ├─────────────────────────────────────────────────────────►│  SQL Server Implementer
    │◄─────────────────────────────────────────────────────────┤  ✅ Script CREATE TABLE + docs
    │                                                          │
    │  === PASO 3.2: REPOSITORY TESTS ===                      │
    │  🚀 Invoca TDD Implementer (tests de repositorios)       │
    ├─────────────────────────────────────────────────────────►│  TDD Implementer
    │◄─────────────────────────────────────────────────────────┤  ✅ CategoryRepositoryTests creado
    │                                                          │
    │  ✅ CHECKPOINT: dotnet test ── pasa                      │
    │                                                          │
    │  === PASO 4: API ===                                     │
    │  🚀 Invoca API Implementer                               │
    ├─────────────────────────────────────────────────────────►│  API Implementer
    │◄─────────────────────────────────────────────────────────┤  ✅ CategoryController creado
    │                                                          │
    │  ✅ CHECKPOINT: dotnet build && dotnet test               │
    │                                                          │
    │  🔍 VERIFICACIÓN CONTRATO OpenAPI:                       │
    │  Ejecuta la API, descarga swagger.json y verifica que    │
    │  los endpoints paginados listan pageNumber, pageSize,    │
    │  sort como query params. Si discrepan → reinvoca API     │
    │  Implementer.                                            │
    │                                                          │
    │  === PASO 5: CODE REVIEW + COBERTURA ===                 │
    │  🚀 Invoca Code Reviewer (solo lectura)                  │
    ├─────────────────────────────────────────────────────────►│  Code Reviewer
    │◄─────────────────────────────────────────────────────────┤  📋 Reporte: APROBADO / NECESITA CAMBIOS
    │                                                          │
    │  Si NECESITA CAMBIOS → delega correcciones al           │
    │  sub-agente de la capa afectada (NUNCA corrige directo)  │
    │                                                          │
    │  🚀 Invoca Coverage Analyzer                             │
    ├─────────────────────────────────────────────────────────►│  Coverage Analyzer
    │◄─────────────────────────────────────────────────────────┤  📋 Reporte: ≥95% / <95%
    │                                                          │
    │  Si <95% → reinvoca TDD Implementer con lista de        │
    │  métodos sin cubrir (máx 3 iteraciones)                  │
    │                                                          │
    │  === PASO 5.5: SPEC COMPLIANCE ===                       │
    │  🔍 Invoca Spec Compliance Verifier                      │
    │  Contexto: "Verifica implementación contra spec"         │
    ├─────────────────────────────────────────────────────────►│  Spec Compliance Verifier
    │                                                          │  - Lee spec original
    │                                                          │  - Compara con código + swagger
    │                                                          │  - Genera matriz de trazabilidad
    │◄─────────────────────────────────────────────────────────┤  📋 Veredicto: CUMPLE / ADVERTENCIAS
    │                                                          │
    │  Si NO CUMPLE → delega correcciones al sub-agente        │
    │  de la capa afectada (máx 2 iteraciones)                 │
    │                                                          │
    │  ✅ CHECKPOINT FINAL: dotnet build && dotnet test        │
```

### Fase 5 — Pull Request (PR Builder)

**Objetivo**: Redactar el Pull Request de la feature, mostrárselo al desarrollador para revisión y publicarlo en Azure DevOps **solo tras aprobación explícita**.

**Se activa**: Invocando `/create-pr {ID}` en una **nueva sesión** después de que la implementación está completa y los tests pasan.

```
Developer                         PR Builder
    │                                  │
    │  "/create-pr 1234"               │
    ├─────────────────────────────────►│
    │                                  │  Ejecuta git status --short
    │                                  │
    │  (si hay cambios pendientes)     │
    │  "Hay X archivos sin commitear.  │
    │  Mensaje propuesto:              │
    │  feat: ... (#1234)               │
    │  ¿Apruebas?"                     │
    │◄─────────────────────────────────┤
    │  "Aprobado ✅"                    │
    ├─────────────────────────────────►│
    │                                  │  git add -A && git commit -m "..."
    │                                  │
    │                                  │  Lee specs/active/1234-*/
    │                                  │  (specification.md, tasks.md)
    │                                  │  Lee git log (commits del branch)
    │                                  │  Consulta rama actual y rama destino
    │                                  │
    │                                  │  Redacta borrador del PR:
    │                                  │  - Título
    │                                  │  - Descripción (qué, por qué, cómo)
    │                                  │  - Work Item vinculado (#{ID})
    │                                  │  - Checklist de revisión
    │                                  │
    │  📋 "Este es el PR que voy a     │
    │  crear. Revísalo antes de        │
    │  publicar:                       │
    │                                  │
    │  **Título:** [título]            │
    │  **Rama origen:** feature/1234   │
    │  **Rama destino:** main          │
    │  **Work Item:** #1234            │
    │                                  │
    │  **Descripción:**                │
    │  [descripción completa]          │
    │                                  │
    │  ¿Apruebas este PR tal como      │
    │  está, o quieres ajustarlo?"     │
    │◄─────────────────────────────────┤
    │                                  │
    │  (Developer revisa, puede pedir  │
    │   cambios en título/descripción) │
    │  "Aprobado ✅" / "Cambia X por Y"│
    ├─────────────────────────────────►│
    │                                  │  Si hay cambios: aplica y muestra
    │                                  │  de nuevo el borrador actualizado.
    │                                  │
    │                                  │  Si aprobado: ejecuta
    │                                  │  az repos pr create ...
    │                                  │
    │  🔀 "PR creado exitosamente:     │
    │  https://dev.azure.com/...       │
    │  PR #456 - Esperando revisión"   │
    │◄─────────────────────────────────┤
    │                                  │
    │                                  │  Mueve specs/active/1234-*/
    │                                  │  → specs/completed/1234-*/
```

**Comportamiento clave:**

- **Nunca publica sin aprobación.** El agente muestra el borrador completo y espera un "Aprobado ✅" explícito del desarrollador antes de ejecutar cualquier comando `az repos pr create`.
- **Permite iteración.** Si el desarrollador pide cambios en el título o descripción, el agente actualiza el borrador y lo muestra de nuevo antes de publicar.
- **Vincula automáticamente el Work Item.** Usa el `{ID}` para agregar `AB#1234` en la descripción, lo que vincula el PR al Work Item de Azure DevOps.
- **Mueve los artefactos a `completed/`.** Tras crear el PR exitosamente, mueve la carpeta `specs/active/{ID}-*/` a `specs/completed/`.

**Herramientas requeridas:**
- MCP `ado` (`@azure-devops/mcp`) — ya configurado en `.vscode/mcp.json` con la organización `olimpiait`. El agente usa sus herramientas nativas para leer Work Items y crear PRs.
- Fallback: `az repos pr create` si el MCP no soporta la operación.

---

## 5. Estructura de Archivos

```
.github/
├── copilot-instructions.md                          # 📌 Instrucciones globales (siempre activas)
│
├── agents/                                          # 🤖 16 agentes (5 principales + 11 sub-agentes)
│   ├── spec-builder.agent.md                        #    Principal: Construye specs
│   ├── plan-builder.agent.md                        #    Principal: Crea planes
│   ├── task-definer.agent.md                        #    Principal: Define tareas
│   ├── orchestrator.agent.md                        #    Principal: Orquesta implementación
│   ├── pr-builder.agent.md                          #    Principal: Redacta y publica PR
│   ├── codebase-explorer.agent.md                   #    Sub: Exploración unificada
│   ├── tdd-implementer.agent.md                     #    Sub: Ciclo TDD Completo (Red/Green/Refactor)
│   ├── domain-implementer.agent.md                  #    Sub: Capa Domain
│   ├── application-implementer.agent.md             #    Sub: Capa Application
│   ├── infrastructure-implementer.agent.md          #    Sub: Capa Infrastructure
│   ├── api-implementer.agent.md                     #    Sub: Capa Api
│   ├── sql-server-implementer.agent.md              #    Sub: Scripts SQL Server
│   ├── code-reviewer.agent.md                       #    Sub: Revisión de código│   ├─ spec-compliance-verifier.agent.md             #    Sub: Verificación spec↔código│   ├── coverage-analyzer.agent.md                   #    Sub: Análisis de cobertura ≥95%
│   └── doc-updater.agent.md                         #    Sub: Actualización de docs
│
├── instructions/                                    # 📏 Reglas por contexto
│   ├── api-controllers.instructions.md              #    Se aplica a Controllers
│   ├── api-middleware.instructions.md               #    Se aplica a Middleware
│   ├── api-program.instructions.md                  #    Se aplica a Program.cs
│   ├── api-auth.instructions.md                     #    Se aplica a Autenticación
│   ├── api-pagination.instructions.md               #    Se aplica a Queries paginadas, Validators y Controllers
│   ├── api-xmldocs.instructions.md                  #    XML docs y [ProducesResponseType] en Controllers + Commands/Queries/DTOs
│   ├── cqrs-commands.instructions.md                #    Se aplica a Commands
│   ├── cqrs-queries.instructions.md                 #    Se aplica a Queries
│   ├── cqrs-validators.instructions.md              #    Se aplica a Validators
│   ├── csharp-conventions.instructions.md           #    Se aplica a **/*.cs
│   ├── data-access-repositories.instructions.md     #    Se aplica a Repositorios
│   ├── data-access-sqlkata.instructions.md          #    Se aplica a SqlKata
│   ├── data-access-unitofwork.instructions.md       #    Se aplica a UnitOfWork
│   ├── data-access-sp-views.instructions.md         #    Se aplica a SPs y Vistas
│   ├── database.instructions.md                     #    Se aplica a **/*.sql
│   ├── domain-entities.instructions.md              #    Se aplica a Entidades
│   ├── domain-interfaces.instructions.md            #    Se aplica a Interfaces de Dominio
│   ├── feature-caching.instructions.md              #    Se aplica a Caché
│   ├── feature-http-clients.instructions.md         #    Se aplica a HTTP Clients
│   ├── feature-logging.instructions.md              #    Se aplica a Logging
│   ├── testing-handlers.instructions.md             #    Se aplica a Tests de Handlers
│   ├── testing-repositories.instructions.md         #    Se aplica a Tests de Repositorios
│   ├── testing-validators.instructions.md           #    Se aplica a Tests de Validators
│   └── testing-fixtures.instructions.md             #    Se aplica a Fixtures
│
├── skills/                                          # 🛠️ Habilidades reutilizables
│   ├── caching/SKILL.md                             #    Caché distribuida con Redis
│   ├── clean-arch-validation/SKILL.md               #    Validación de arquitectura
│   ├── external-api/SKILL.md                        #    Llamadas HTTP externas + Polly
│   ├── new-feature/SKILL.md                         #    Checklist end-to-end de nuevo feature
│   ├── stored-procedures-views/SKILL.md             #    SPs y Views con repositorios
│   ├── tdd-workflow/SKILL.md                        #    TDD Red→Green→Refactor + testing
│   └── logging/SKILL.md                             #    Documentación profunda del logger
│
├── hooks/                                           # 🪝 Automatización
│   └── quality-gates.json                           #    Configuración de hooks
│
├── prompts/                                         # ⚡ Comandos rápidos
│   ├── spec-from-story.prompt.md                    #    /spec-from-story
│   ├── plan-from-spec.prompt.md                     #    /plan-from-spec
│   ├── tasks-from-plan.prompt.md                    #    /tasks-from-plan
│   ├── implement-tasks.prompt.md                    #    /implement-tasks
│   └── create-pr.prompt.md                          #    /create-pr

scripts/hooks/                                       # 🔧 Scripts de hooks (cross-platform)
├── validate-command.ps1                             #    Bloquea comandos peligrosos (Windows)
├── validate-command.sh                              #    Bloquea comandos peligrosos (Linux/macOS)
├── subagent-log.ps1                                 #    Notifica inicio de sub-agentes (Windows)
└── subagent-log.sh                                  #    Notifica inicio de sub-agentes (Linux/macOS)

specs/                                               # 📁 Artefactos generados por feature
├── active/                                          #    Features en desarrollo
│   └── {ID}-{feature-name}/                         #    Carpeta por Work Item de Azure DevOps
│       ├── specification.md                         #    Especificación técnica
│       ├── plan.md                                  #    Plan de implementación
│       └── tasks.md                                 #    Definición de tareas
├── completed/                                       #    Features terminadas (movidas por PR Builder)
└── templates/                                       #    Plantillas de artefactos
    ├── specification-template.md                    #    Plantilla de especificación
    ├── plan-template.md                             #    Plantilla de plan
    └── tasks-template.md                            #    Plantilla de tareas
```

---

## 6. Componentes del Framework

### 6.1 Instructions (Instrucciones)
Las instrucciones son reglas granulares que Copilot sigue **automáticamente** sin necesidad de mencionarlas, protegidas por patrones de alcance (`applyTo`) para no saturar el contexto. (Ver estructura de archivos).

### 6.2 Skills (Habilidades)
Las skills son guías de referencia que los agentes pueden consultar cuando necesitan saber **cómo** hacer algo complejo bajo demanda.

### 6.3 Templates (Plantillas)
Estandarizan la salida documental para Spec, Plan y Tasks.

### 6.4 Hooks (Automatización)
Los hooks ejecutan automatizaciones, por ejemplo bloquear comandos peligrosos.

### 6.5 Prompts (Comandos Rápidos)
Comandos rápidos como `/spec-from-story` o `/implement-tasks`.

---

## 7. Transiciones entre Fases

Las transiciones entre fases se realizan **abriendo una nueva sesión de chat** con el prompt correspondiente (`/plan-from-spec`, `/tasks-from-plan`, `/implement-tasks`, `/create-pr`). Esto preserva contexto limpio en cada fase.

Los agentes Plan Builder y Task Definer tienen **handoffs backward** (botones de retroceso) para volver a la fase anterior si se detectan problemas en la spec o el plan. No hay handoffs forward — el developer controla cuándo avanzar.

---

## 8. Hooks — Eventos y Automatización

La arquitectura usa hooks `SubagentStart` para logs y `PreToolUse` para seguridad (verificando comandos ejecutados en terminal). La verificación de test y build se orquesta desde los propios agentes y no requiere un hook invasivo.

---

## 9. Ciclo TDD (Red → Green → Refactor)

```
  ┌───────────────────────────────────────────────┐
  │              CICLO TDD POR TAREA              │
  │                                               │
  │   🤖 TDD Implementer                          │
  │   • Fase 1: Escribe tests que FALLAN          │
  │   • Fase 2: Implementación MÍNIMA para pasar  │
  │   • Fase 3: Refactoriza sin romper tests      │
  │   • Ejecuta dotnet test continuamente         │
  │              │                                │
  │              ▼                                │
  │   ✅ CHECKPOINT: Todos los tests pasan         │
  │                                               │
  └───────────────────────────────────────────────┘
```

**¿Por qué un solo agente TDD Implementer?**
Evita pasar el estado intermedio de un test fallido, al código de implementación, a un tercer agente de refactor. Esto **ahorra muchísimos tokens y turnos** en la comunicación del Orquestador, mitigando la pérdida de contexto que ocurre al cruzar las barreras de las ventanas de los agentes.

---

## 10. Checkpoints Verificables

Cada fase tiene checkpoints que deben cumplirse antes de avanzar:
- **Domain:** Build compila sin errores.
- **TDD:** Tests pasan. Cada test tiene **un assert lógico** (usar `BeEquivalentTo` para DTOs completos).
- **Api:** Endpoints paginados exponen `pageNumber`, `pageSize`, `sort` como `[FromQuery]` en Swagger. Filtros dinámicos documentados via `PaginatedEndpointOperationFilter`.
- **Contrato OpenAPI:** Verificar `swagger.json` — todos los query params de la spec (§10 "Query Params") visibles como parámetros del endpoint. Sin discrepancias spec↔schema.
- **XML Docs:** Controllers y DTOs expuestos con `<summary>`, `<remarks>`, `<response>` y `[ProducesResponseType]` por cada código HTTP posible. Ver [`docs/API_DOCUMENTATION.md`](docs/API_DOCUMENTATION.md).
- **Manejo de errores:** `ExceptionHandlingMiddleware` registrado en `Program.cs`. Controllers **sin try/catch** — excepciones traducidas a `ProblemDetails` tipado.
- **Code Style (A1–A18):** Sin comparaciones `== true/false`; clases concretas `sealed`; abreviaturas en PascalCase .NET (`Id`, `Url`, `Http`, `Sql`, `Api`); sin `return null` sorpresa; ver [`docs/PATTERNS.md §7`](docs/PATTERNS.md#7-convenciones-de-código-c-code-style).
- **Spec Compliance:** Veredicto CUMPLE o CUMPLE CON ADVERTENCIAS del Spec Compliance Verifier. Cada RF, CA, RN de la spec tiene evidencia de implementación.
- **Cobertura:** ≥95% line coverage en archivos nuevos.

---

## 11. Mecanismos de Seguridad

- Comandos Bloqueados (`rm -rf /`, `DROP TABLE`, etc.)
- Verificación Continua (El Orchestrator ejecuta `dotnet build`/`test` tras cada sub-agente).
- Resolución Cíclica de Errores Cross-Layer para evitar bucles infinitos.

---

## 12. Ejemplo 1 — CRUD de Categorías

> **Dificultad**: ⭐ Básico — Feature CRUD simple, ideal para entender el flujo completo.

### Historia de Usuario

```
COMO administrador del sistema
QUIERO poder crear, consultar, actualizar y eliminar categorías de productos
PARA poder clasificar los productos del catálogo

Criterios de Aceptación:
- Puedo crear una categoría con nombre (requerido, max 100 chars) y descripción (opcional, max 500 chars)
- El nombre de la categoría debe ser único
- Puedo consultar una categoría por su ID
- Puedo listar todas las categorías
- Puedo actualizar el nombre y descripción de una categoría
- Puedo eliminar una categoría si no tiene productos asociados
```

### Paso a Paso

**1. Inicia el Spec Builder:**

Abre el chat de Copilot → escribe `/spec-from-story` → ingresa el ID del Work Item (ej: `1234`) y pega la historia de usuario.

```
Tú: "Tengo esta historia de usuario de Azure DevOps:
     COMO administrador del sistema QUIERO poder crear, consultar,
     actualizar y eliminar categorías de productos..."
```

El agente te hará preguntas como:
- "¿La categoría tiene alguna relación jerárquica (padre-hijo)?"
- "¿El endpoint de listar debe soportar paginación?"
- "¿Qué políticas de autorización se requieren?"

Responde y cuando estés conforme, aprueba. Se genera `specs/active/1234-crud-categorias/specification.md`.

**2. Usa `/plan-from-spec 1234`:**

El Plan Builder lee la spec y crea un plan con archivos concretos:

```
Fase 1 — Domain:
  - Crear src/Olimpia.Domain/Entities/Category.cs
  - Crear src/Olimpia.Domain/Repositories/ICategoryRepository.cs

Fase 1.5 — Application Scaffolding:
  - Crear src/Olimpia.Application/Categories/Commands/CreateCategory/CreateCategoryCommand.cs
  - Crear src/Olimpia.Application/Categories/Queries/GetCategoryById/GetCategoryByIdQuery.cs
  - Crear src/Olimpia.Application/Categories/Queries/GetCategoryById/GetCategoryByIdDto.cs

Fase 2 — TDD (Handlers y Validators):
  - Crear tests/Olimpia.Tests/Handlers/Categories/CreateCategoryHandlerTests.cs
  - Crear tests/Olimpia.Tests/Handlers/Categories/UpdateCategoryHandlerTests.cs
  - Crear tests/Olimpia.Tests/Handlers/Categories/DeleteCategoryHandlerTests.cs
  - Crear tests/Olimpia.Tests/Handlers/Categories/GetCategoryByIdHandlerTests.cs
  - Crear tests/Olimpia.Tests/Validators/CreateCategoryValidatorTests.cs
  ...
```

Aprueba el plan. Se genera `specs/active/1234-crud-categorias/plan.md`.

**3. Usa `/tasks-from-plan 1234`:**

El Task Definer descompone en tareas atómicas:

```
T-001 | Domain     | Category.cs (entity)              | sin dependencias
T-002 | Domain     | ICategoryRepository.cs (interface) | depende de T-001
T-003 | Application| CreateCategoryCommand.cs (record)  | depende de T-001
T-004 | Application| GetCategoryByIdQuery.cs + Dto      | depende de T-001
T-005 | Tests+App  | CreateCategoryTests + Handler + Validator (TDD) | depende de T-003, T-002
T-006 | Tests+App  | GetCategoryByIdTests + Handler (TDD)            | depende de T-004, T-002
...
```

Aprueba las tareas. Se genera `specs/active/1234-crud-categorias/tasks.md`.

**4. Usa `/implement-tasks 1234`:**

El Orchestrator comienza a ejecutar. Verás en el chat:

```
──────────────────────────────────────────────────
🚀 [14:23:01] SUB-AGENTE INICIADO: Domain Implementer
──────────────────────────────────────────────────
   Creando Category.cs...
──────────────────────────────────────────────────
✅ [14:23:15] SUB-AGENTE FINALIZADO: Domain Implementer
──────────────────────────────────────────────────

🔨 Verificando build... ✅ Build exitoso.

──────────────────────────────────────────────────
🚀 [14:23:18] SUB-AGENTE INICIADO: Application Implementer
──────────────────────────────────────────────────
   Creando CreateCategoryCommand, GetCategoryByIdQuery, DTO...
──────────────────────────────────────────────────
✅ [14:23:25] SUB-AGENTE FINALIZADO: Application Implementer
──────────────────────────────────────────────────

🔨 Verificando build... ✅ Build exitoso.

──────────────────────────────────────────────────
🚀 [14:23:28] SUB-AGENTE INICIADO: TDD Implementer
──────────────────────────────────────────────────
   🔴 Escribiendo CreateCategoryTests.cs (Red)...
   🟢 Implementando CreateCategoryHandler (Green)...
   🔵 Refactorizando...
──────────────────────────────────────────────────
✅ [14:24:05] SUB-AGENTE FINALIZADO: TDD Implementer
──────────────────────────────────────────────────

🧪 Tests: 4 pasando ✅

──────────────────────────────────────────────────
🔍 [14:25:30] SUB-AGENTE INICIADO: Spec Compliance Verifier
──────────────────────────────────────────────────
   Verificando RF-01..RF-06, CA-01..CA-06, RN-01..RN-03 contra código...
──────────────────────────────────────────────────
✅ [14:25:50] SUB-AGENTE FINALIZADO: Spec Compliance Verifier
──────────────────────────────────────────────────

📋 Veredicto: CUMPLE — 6/6 RF, 6/6 CA, 3/3 RN ✅
...
```

### Archivos Resultantes

```
src/Olimpia.Domain/Entities/Category.cs
src/Olimpia.Domain/Repositories/ICategoryRepository.cs
src/Olimpia.Application/Categories/Commands/CreateCategory/CreateCategoryCommand.cs
src/Olimpia.Application/Categories/Commands/CreateCategory/CreateCategoryHandler.cs
src/Olimpia.Application/Categories/Commands/CreateCategory/CreateCategoryValidator.cs
src/Olimpia.Application/Categories/Queries/GetCategoryById/GetCategoryByIdQuery.cs
src/Olimpia.Application/Categories/Queries/GetCategoryById/GetCategoryByIdHandler.cs
src/Olimpia.Application/Categories/Queries/GetCategoryById/GetCategoryByIdDto.cs
src/Olimpia.Infrastructure/Persistence/Repositories/CategoryRepository.cs
src/Olimpia.Api/Controllers/CategoryController.cs
tests/Olimpia.Tests/Handlers/Categories/CreateCategoryHandlerTests.cs
tests/Olimpia.Tests/Handlers/Categories/UpdateCategoryHandlerTests.cs
tests/Olimpia.Tests/Handlers/Categories/DeleteCategoryHandlerTests.cs
tests/Olimpia.Tests/Handlers/Categories/GetCategoryByIdHandlerTests.cs
tests/Olimpia.Tests/Validators/CreateCategoryValidatorTests.cs
tests/Olimpia.Tests/Repositories/CategoryRepositoryTests.cs
specs/active/1234-crud-categorias/specification.md
specs/active/1234-crud-categorias/plan.md
specs/active/1234-crud-categorias/tasks.md
```

---

## 13. Ejemplo 2 — Query de Búsqueda con Filtros

> **Dificultad**: ⭐⭐ Intermedio — Solo queries, sin commands. Incluye filtros dinámicos y paginación.

### Historia de Usuario

```
COMO usuario del sistema
QUIERO poder buscar productos por nombre, categoría y rango de precios
PARA encontrar rápidamente los productos que necesito

Criterios de Aceptación:
- Puedo buscar por nombre (contiene, case-insensitive)
- Puedo filtrar por categoryId (opcional)
- Puedo filtrar por rango de precios: precioMinimo y precioMaximo (opcionales)
- Los resultados están paginados (page, pageSize, default 20, max 100)
- La respuesta incluye: items, totalCount, page, pageSize, totalPages
- Si no hay resultados, retorna lista vacía (no error)
```

### Paso a Paso

**1. Spec Builder** → Usa `/spec-from-story` con el ID del Work Item y pega la HU. El agente preguntará cosas como:
- "¿El filtro por nombre debe buscar en nombre y descripción, o solo nombre?"
- "¿Se requiere ordenamiento (sort by name, price, date)?"
- "¿El endpoint requiere autenticación?"

**2. Plan Builder** → Usa `/plan-from-spec {ID}`. Como es solo un Query (sin Command), el plan será más corto:
- No se crea entidad nueva (Product ya existe)
- Se agrega un nuevo método al `IProductRepository` (o se crea `ICategoryRepository` si no existe)
- Se crea `SearchProductsQuery`, `SearchProductsHandler`, `SearchProductsDto`, `SearchProductsValidator`

**3. Task Definer** → Usa `/tasks-from-plan {ID}`. Tareas orientadas a Query:

```
T-001 | Domain     | Agregar método SearchAsync a IProductRepository
T-002 | Application| SearchProductsQuery + Dto (scaffolding)
T-003 | Tests+App  | SearchProductsTests + Handler + Validator (TDD)
T-004 | Infra      | Implementar SearchAsync en ProductRepository con SqlKata
T-005 | Api        | Agregar endpoint GET /api/products/search en ProductController
```

**4. Orchestrator** → Usa `/implement-tasks {ID}`. Ejecuta con TDD:
- **Domain Implementer** agrega el método a la interfaz
- **Application Implementer** crea el Query record + DTO
- **TDD Implementer** escribe tests y handler+validator (Red→Green→Refactor)
- **Infrastructure Implementer** implementa `SearchAsync` con SqlKata (query dinámico con filtros opcionales)
- **API Implementer** agrega el endpoint al controller existente
- **Spec Compliance Verifier** verifica que los filtros, paginación y respuesta coinciden con la spec

### Lo Interesante de Este Ejemplo

- Demuestra que el flujo funciona para **modificar archivos existentes** (no solo crear nuevos)
- El `ProductRepository.cs` existente se **extiende** con un nuevo método
- El `ProductController.cs` existente recibe un nuevo endpoint
- SqlKata permite queries dinámicos: `.WhereIf(condition, "Column", value)`

---

## 14. Ejemplo 3 — Feature con Integración HTTP Externa

> **Dificultad**: ⭐⭐⭐ Avanzado — Incluye llamada HTTP externa, retry con Polly, y caché Redis.

### Historia de Usuario

```
COMO sistema
QUIERO validar los datos fiscales de un proveedor contra la API externa del SAT
PARA garantizar que solo se registren proveedores con datos fiscales válidos

Criterios de Aceptación:
- Al crear un proveedor, se valida su RFC contra la API del SAT (https://api.sat.gob.mx/validate)
- Si la API del SAT no responde en 5 segundos, se reintenta hasta 3 veces con backoff exponencial
- Si después de 3 reintentos la API no responde, se registra el proveedor con estado "PendingValidation"
- El resultado de la validación se cachea en Redis por 24 horas (para evitar llamadas repetidas)
- El endpoint de creación retorna el estado de validación fiscal
```

### Paso a Paso

**1. Spec Builder** → Usa `/spec-from-story` con el ID del Work Item. Preguntas clave que hará:
- "¿Cuál es el contrato exacto de la API del SAT? (URL, headers, body, response)"
- "¿El caché Redis usa el mismo connection string que ya está configurado?"
- "¿Qué sucede si el RFC ya fue validado previamente y está en caché?"
- "¿Se necesita un endpoint para re-validar manualmente?"

**2. Plan Builder** → Usa `/plan-from-spec {ID}`. Plan más complejo con componentes adicionales:

```
Fase 1 — Domain:
  - Crear entidad Supplier (con campo FiscalValidationStatus)
  - Crear ISupplierRepository
  - Crear interfaz ISatValidationService (en Domain — es un puerto)

Fase 1.5 — Application Scaffolding:
  - Crear CreateSupplierCommand, CreateSupplierDto

Fase 2 — TDD (Handlers y Validators):
  - Tests de CreateSupplierHandler (mock de ISatValidationService)
  - Tests de SatValidationService (mock de HttpClient)
  - Tests de escenarios: SAT responde OK, SAT timeout → PendingValidation, RFC en caché

Fase 3 — Infrastructure:
  - SupplierRepository (Dapper + SqlKata)
  - SatValidationService (HttpClient + Polly retry)
  - SatValidationCacheDecorator (Redis)
  - Registro DI con Scrutor decorators

Fase 4 — Api:
  - SupplierController con endpoint POST /api/suppliers
```

**3. Task Definer** → Usa `/tasks-from-plan {ID}`. Más tareas por la complejidad:

```
T-001 | Domain     | Supplier.cs (entity con FiscalValidationStatus)
T-002 | Domain     | ISupplierRepository.cs
T-003 | Domain     | ISatValidationService.cs (puerto)
T-004 | Application| CreateSupplierCommand.cs + Dto (scaffolding)
T-005 | Tests+App  | CreateSupplierTests (happy path) + Handler + Validator (TDD)
T-006 | Tests+App  | CreateSupplierTests (SAT timeout) (TDD)
T-007 | Tests+App  | CreateSupplierTests (RFC en caché) (TDD)
T-008 | Tests      | SatValidationServiceTests.cs (TDD)
T-009 | Infra      | SatValidationService (HttpClient + Polly)
T-010 | Infra      | SatValidationCacheDecorator (Redis)
T-011 | Infra      | SupplierRepository
T-012 | Infra      | DI registration (Scrutor + decorators)
T-013 | Api        | SupplierController
T-014 | Refactor   | Limpieza general
T-015 | Docs       | Actualizar README con nuevo feature
```

**4. Orchestrator** → Usa `/implement-tasks {ID}`. Lo interesante aquí:
- **Application Implementer** crea los Command/Query records y DTOs (scaffolding)
- **TDD Implementer** implementa handlers y validators con ciclo Red→Green→Refactor
- **Infrastructure Implementer** crea el `SatValidationService` con Polly:
  ```
  Policy.Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(3, attempt =>
      TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)))
  ```
- **Infrastructure Implementer** crea el `SatValidationCacheDecorator` que envuelve el servicio con Redis
- Scrutor registra la cadena de decorators: `ISatValidationService` → Cache → Retry → Real

### Lo Interesante de Este Ejemplo

- Demuestra el patrón **Decorator** (Scrutor) con múltiples capas: Cache → Retry → Service
- Demuestra integración con **HttpClient** y **Polly** para resiliencia
- Demuestra uso de **Redis** para caché distribuida
- El Orchestrator debe coordinar más sub-agentes que en los ejemplos anteriores
- La **especificación** es crucial aquí porque hay muchos edge cases que definir antes de implementar
- El **Spec Compliance Verifier** valida los 3 escenarios (SAT OK, SAT timeout → PendingValidation, RFC en caché) contra los criterios de aceptación Gherkin

---

## 15. Preguntas Frecuentes

### ¿Cómo inicio el flujo?

Abre el chat de Copilot → Escribe `/spec-from-story` → Pega tu ID de Work Item y HU.

### ¿Puedo saltar fases?
Sí, los agentes leen el estado desde los archivos markdown en `specs/active/`. Puedes usar `/implement-tasks {ID}` si ya tienes las tareas creadas manualmente.

### ¿Cómo publico el Pull Request?

Una vez que la implementación está completa y los tests pasan, abre una nueva sesión y usa `/create-pr {ID}`. El **PR Builder** leerá la spec y el historial de commits para redactar el PR (título, descripción, work item vinculado) y te lo mostrará en el chat **antes de publicar nada**. Solo cuando confirmes con "Aprobado ✅" ejecutará el comando `az repos pr create`. Puedes pedir ajustes al borrador tantas veces como necesites.

> **Prerrequisito:** El MCP `@azure-devops/mcp` configurado en `.vscode/mcp.json` (ya está configurado en este proyecto con la organización `olimpiait`). Si el MCP no pudiera crear el PR, el agente usará `az repos pr create` como alternativa.

### ¿Qué modelos de IA se usan?
- **Análisis y diseño** (Spec Builder, Plan Builder): `Claude Opus 4.6` — alto razonamiento para requisitos complejos.
- **Revisión de código** (Code Reviewer): `Claude Opus 4.6` — máxima precisión para detectar bugs y violaciones.
- **Verificación de cumplimiento** (Spec Compliance Verifier): `Claude Sonnet 4.6` — equilibrio razonamiento/velocidad para cross-referenciar spec↔código.
- **Implementación compleja** (Orchestrator, TDD Implementer, Application Implementer, Infrastructure Implementer, API Implementer, SQL Server Implementer, PR Builder, Domain Implementer, Doc Updater, Task Definer): `Claude Sonnet 4.6` — equilibrio razonamiento/velocidad.
- **Tareas estructuradas simples** (Codebase Explorer, Coverage Analyzer): `Claude Haiku 4.5` — velocidad para scaffolding y exploración.

### ¿Cuánto contexto consume cada sub-agente?
Cada sub-agente recibe **solo el contexto de su tarea** (el archivo que debe crear/modificar y reglas de su capa). Al terminar, devuelve un resumen breve al Orchestrator. Esto mantiene la ventana principal limpia y previene la "alucinación" por desbordamiento de tokens.
