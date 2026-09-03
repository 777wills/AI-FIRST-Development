---
name: tasks-from-plan
description: Define tareas de implementación a partir de un plan aprobado.
argument-hint: "ID del Work Item — ej: 1234"
agent: Task Definer
---

# Tareas desde Plan Aprobado

Lee el plan aprobado del Work Item **${input:workItemId:ID del Work Item (solo el número, ej: 1234)}** ubicado en `specs/active/{ID}-*/plan.md` y define las tareas de implementación detalladas.

## Instrucciones

1. Busca el plan con glob `specs/active/${input:workItemId}-*/plan.md`.
2. Verifica que el `status` sea `aprobado` antes de continuar.
3. Usa la plantilla de tareas en [tasks-template.md](../../specs/templates/tasks-template.md).
4. Explora el codebase para verificar estado actual y detectar conflictos.
5. Guarda las tareas en `specs/active/{ID}-{feature-name}/tasks.md`.
