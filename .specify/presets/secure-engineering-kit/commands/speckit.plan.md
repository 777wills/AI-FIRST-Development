---
description: Ejecuta el flujo de planificación de implementación para definir la arquitectura, el stack tecnológico y los artefactos de diseño.
handoffs:
  - label: Crear tareas
    agent: speckit.tasks
    prompt: Descompón el plan en tareas
    send: true
  - label: Crear checklist
    agent: speckit.checklist
    prompt: Crea una checklist para el dominio definido...
scripts:
  sh: scripts/bash/setup-plan.sh --json
  ps: scripts/powershell/setup-plan.ps1 -Json
---

# Entrada del usuario

```text
$ARGUMENTS
```

Si el usuario proporciona información adicional, intégrala durante la planificación.

## Regla de idioma (neutral/adaptativa)

- Responde en el idioma de la solicitud del usuario.
- Si el idioma no está claro, conserva el idioma predominante del artefacto que estás editando.
- No impongas un idioma global para todo el flujo.

## Validaciones previas

### Hooks de extensión (antes de planificar)

- Verifica si existe `.specify/extensions.yml`.
- Si existe, procesa `hooks.before_plan`.
- Ignora silenciosamente archivos inexistentes o YAML inválidos.
- Considera deshabilitados únicamente los hooks con `enabled: false`.
- No evalúes expresiones de `condition`; esa responsabilidad corresponde al HookExecutor.
- Para cada hook ejecutable:

  - **Opcional (`optional: true`)**

    ```text
    ## Extension Hooks

    **Optional Pre-Hook**: {extension}
    Command: `/{command}`
    Description: {description}

    Prompt: {prompt}
    To execute: `/{command}`
    ```

  - **Obligatorio (`optional: false`)**

    ```text
    ## Extension Hooks

    **Automatic Pre-Hook**: {extension}
    Executing: `/{command}`
    EXECUTE_COMMAND: {command}
    ```

    Después de emitir el bloque debes ejecutar el hook y esperar su resultado antes de continuar.

Si no existen hooks registrados, continúa sin informar nada.

# Flujo

## 1. Preparación

Ejecuta `{SCRIPT}` desde la raíz del repositorio y procesa el JSON resultante para obtener:

- FEATURE_SPEC
- IMPL_PLAN
- SPECS_DIR
- BRANCH

## 2. Cargar contexto

Lee:

- FEATURE_SPEC
- `FEATURE_DIR/security-context.md` (obligatorio)
- `/memory/constitution.md`
- La plantilla IMPL_PLAN

Si `FEATURE_DIR/security-context.md` no existe:

- no reconstruyas perfiles completos por defecto;
- solicita ejecutar `/specify` para regenerar el resumen operativo;
- solo continúa sin ese archivo si el usuario lo autoriza explícitamente.

## 3. Ejecutar la planificación

Completa la plantilla siguiendo este orden:

### Contexto técnico

- Completa toda la información disponible.
- Marca cualquier dato desconocido como **NEEDS CLARIFICATION**.

### Verificación de la constitución

- Completa la sección correspondiente utilizando `constitution.md`.
- Usa `security-context.md` como fuente principal para REQ y trazabilidad de seguridad.
- Si existe alguna violación no justificada, detén el proceso con **ERROR**.

### Estrategia de pruebas *(obligatorio)*

Completa la sección "Estrategia de Pruebas" del plan siguiendo estas reglas:

- Define los tipos de prueba requeridos para la funcionalidad (unitarias, integración, contrato, aceptación, seguridad).
- Establece los objetivos de cobertura mínimos:
  - Por defecto: ≥ 80 % de líneas, ramas y funciones.
  - Para historias con REQ de seguridad críticos o altos: ≥ 90 %.
  - Si se requiere un umbral inferior, justifícalo explícitamente.
- Documenta la herramienta de cobertura y la ruta de los archivos de prueba.
- Traza al menos un escenario de prueba por historia de usuario hacia los escenarios de aceptación definidos en `spec.md`.
- Traza al menos un test automatizado por cada REQ-* de seguridad de severidad crítica o alta.

Si `spec.md` no contiene escenarios de aceptación verificables, detén el proceso y solicita que se completen antes de continuar.

### Fase 0

Genera `research.md` resolviendo todos los elementos marcados como **NEEDS CLARIFICATION**.

### Fase 1

Genera:

- `data-model.md`
- `contracts/`
- `quickstart.md`

Después actualiza el contexto del agente ejecutando el script correspondiente.

Finalmente vuelve a ejecutar la verificación de la constitución.

# Hooks posteriores

Antes de finalizar:

- Verifica `.specify/extensions.yml`.
- Procesa `hooks.after_plan`.
- Ignora silenciosamente archivos inválidos o inexistentes.
- No evalúes `condition`.
- Ejecuta únicamente los hooks habilitados.

### Hook obligatorio

```text
## Extension Hooks

**Automatic Hook**: {extension}
Executing: `/{command}`
EXECUTE_COMMAND: {command}
```

Ejecuta el hook y espera su finalización.

### Hook opcional

```text
## Extension Hooks

**Optional Hook**: {extension}
Command: `/{command}`
Description: {description}

Prompt: {prompt}
To execute: `/{command}`
```

# Reporte final

Finaliza informando:

- Rama utilizada.
- Ruta del IMPL_PLAN.
- Artefactos generados.
- Estrategia de pruebas definida: tipos seleccionados y objetivos de cobertura.
- Confirmación de que cada historia de usuario tiene al menos un escenario de prueba trazado.
- Confirmación de que cada REQ-* crítico o alto tiene al menos un test automatizado asociado.

# Fases

## Fase 0 — Investigación

Extrae del Contexto Técnico:

- Cada **NEEDS CLARIFICATION** → una tarea de investigación.
- Cada dependencia → mejores prácticas.
- Cada integración → patrones recomendados.

Consolida todo en `research.md` con el formato:

- **Decisión**
- **Justificación**
- **Alternativas consideradas**

No avances mientras exista algún **NEEDS CLARIFICATION** sin resolver.

## Fase 1 — Diseño

Requiere `research.md` completo.

### Modelo de datos

Genera `data-model.md` incluyendo:

- Entidades.
- Campos.
- Relaciones.
- Reglas de validación.
- Estados y transiciones (cuando aplique).

### Contratos

Si existen interfaces públicas, documenta sus contratos dentro de `contracts/`.

Ejemplos:

- APIs
- CLI
- Endpoints
- Esquemas
- Contratos de UI

Omite esta sección si el proyecto es completamente interno.

### Quickstart

Genera `quickstart.md` como guía de validación funcional.

Debe incluir:

- Prerrequisitos.
- Configuración.
- Comandos para ejecutar.
- Resultado esperado.

No incluyas código de implementación.

### Actualización de contexto

Actualiza la referencia del plan entre:

```text
<!-- SPECKIT START -->
...
<!-- SPECKIT END -->
```

dentro de `__CONTEXT_FILE__`.

# Reglas

- Utiliza rutas absolutas para operaciones sobre el sistema de archivos.
- Utiliza rutas relativas al proyecto dentro de la documentación.
- Si existen violaciones de la constitución o aclaraciones pendientes, finaliza con **ERROR**.

# Criterios de finalización

- Plan generado correctamente.
- Artefactos de diseño creados.
- Hooks ejecutados o descartados según corresponda.
- Reporte final entregado.