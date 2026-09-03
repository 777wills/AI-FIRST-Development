---
name: speckit-tasks
description: Genera un `tasks.md` accionable y ordenado por dependencias a partir
  de los artefactos de diseño disponibles.
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: github-spec-kit
  source: preset:secure-engineering-kit
---

# Speckit Tasks Skill

# Entrada del usuario

```text
$ARGUMENTS
```

Si el usuario proporciona información adicional, intégrala durante la generación de tareas.

## Regla de idioma (neutral/adaptativa)

- Responde en el idioma de la solicitud del usuario.
- Si el idioma no está claro, conserva el idioma predominante del artefacto que estás editando.
- No impongas un idioma global para todo el flujo.

# Validaciones previas

## Hooks de extensión (antes de generar tareas)

- Verifica si existe `.specify/extensions.yml`.
- Si existe, procesa `hooks.before_tasks`.
- Ignora silenciosamente archivos inexistentes o YAML inválidos.
- Considera deshabilitados únicamente los hooks con `enabled: false`.
- No evalúes `condition`; esa responsabilidad corresponde al HookExecutor.

Para cada hook ejecutable:

### Hook opcional

```text
## Extension Hooks

**Optional Pre-Hook**: {extension}
Command: `/{command}`
Description: {description}

Prompt: {prompt}
To execute: `/{command}`
```

### Hook obligatorio

```text
## Extension Hooks

**Automatic Pre-Hook**: {extension}
Executing: `/{command}`
EXECUTE_COMMAND: {command}
```

Después de emitir el bloque debes ejecutar el hook y esperar su resultado antes de continuar.

# Flujo

## 1. Preparación

Ejecuta `.specify/scripts/powershell/setup-tasks.ps1 -Json` desde la raíz del repositorio y procesa:

- FEATURE_DIR
- TASKS_TEMPLATE
- AVAILABLE_DOCS

Si `TASKS_TEMPLATE` no existe, utiliza `.specify/templates/tasks-template.md`.

## 2. Cargar artefactos

Desde `FEATURE_DIR` carga:

Obligatorios:

- `plan.md`
- `spec.md`
- `security-context.md`

Opcionales:

- `data-model.md`
- `contracts/`
- `research.md`
- `quickstart.md`

Si existe, carga también:

- `.specify/memory/constitution.md`

Genera las tareas únicamente con la información disponible.

Si `security-context.md` no existe:

- no vuelvas a leer el paquete de seguridad completo por defecto;
- solicita ejecutar `/specify` para regenerarlo;
- solo continúa sin ese archivo con autorización explícita del usuario.

## 3. Generación de tareas

Extrae:

- Stack tecnológico.
- Librerías.
- Estructura del proyecto.
- Historias de usuario y prioridades.
- Entidades.
- Contratos.
- Decisiones de investigación.
- REQ de seguridad y evidencia esperada desde `security-context.md`.
- Objetivos de cobertura de pruebas desde la sección "Estrategia de Pruebas" de `plan.md`.

Genera:

- Tareas organizadas por historia de usuario.
- Grafo de dependencias.
- Oportunidades de ejecución en paralelo.
- Validación de cobertura para cada historia.

## 4. Generar `tasks.md`

Utiliza la plantilla indicada y completa:

- Nombre de la funcionalidad.
- Fase 1: Setup.
- Fase 2: Fundaciones.
- Una fase por historia de usuario (según prioridad).
- Fase final: Pulido y aspectos transversales.

Cada fase debe incluir:

- Objetivo.
- Criterios de validación independiente.
- Pruebas (obligatorio, escritas antes de la implementación; aplica TDD).
- Tareas de implementación.

Incluye además:

- Dependencias.
- Ejemplos de paralelización.
- Estrategia de implementación incremental (MVP primero).

# Hooks posteriores

Antes de finalizar:

- Verifica `.specify/extensions.yml`.
- Procesa `hooks.after_tasks`.
- Ignora archivos inexistentes o inválidos.
- No evalúes `condition`.

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

Informa:

- Ruta de `tasks.md`.
- Número total de tareas.
- Cantidad de tareas por historia de usuario.
- Oportunidades de paralelización.
- Criterios de validación por historia.
- Alcance sugerido para el MVP.
- Objetivos de cobertura de pruebas por historia y cobertura global requerida.
- Confirmación de que cada historia tiene al menos una tarea de prueba antes de su implementación.
- Confirmación de que cada REQ-* crítico o alto tiene al menos una prueba automatizada asociada.
- Confirmación de que todas las tareas cumplen el formato requerido.

Contexto para la generación:

`$ARGUMENTS`

Cada tarea debe ser suficientemente específica para que otro agente pueda ejecutarla sin contexto adicional.

# Reglas para generar tareas

## Organización

Las tareas deben agruparse por historia de usuario para permitir una implementación y validación independientes.

Las tareas de pruebas solo deben generarse cuando hayan sido solicitadas explícitamente o se requiera un enfoque TDD.

## Formato obligatorio

Cada tarea debe seguir exactamente este formato:

```text
- [ ] [TaskID] [P?] [US?] Descripción con ruta del archivo
```

Donde:

- `TaskID`: T001, T002...
- `[P]`: únicamente si puede ejecutarse en paralelo.
- `[USx]`: únicamente en tareas pertenecientes a una historia de usuario.
- La descripción debe incluir la ruta exacta del archivo afectado.

## Organización por fases

### Fase 1

Configuración inicial del proyecto.

### Fase 2

Infraestructura y prerrequisitos compartidos.

### Fases siguientes

Una fase por historia de usuario, siguiendo el orden de prioridad.

Cada fase debe representar un incremento funcional y validable de manera independiente.

Si existen pruebas:

Pruebas → Modelos → Servicios → Interfaces → Integración.

### Fase final

Pulido y tareas transversales.

# Criterios de finalización

- `tasks.md` generado correctamente.
- Hooks ejecutados o descartados según corresponda.
- Reporte final entregado.
