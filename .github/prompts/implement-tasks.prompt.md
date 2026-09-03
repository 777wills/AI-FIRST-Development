---
name: implement-tasks
description: Inicia la implementación orquestada de las tareas definidas con TDD.
argument-hint: "ID del Work Item — ej: 1234"
agent: Orchestrator
---

# Implementar Tareas con TDD

Lee las tareas aprobadas del Work Item **${input:workItemId:ID del Work Item (solo el número, ej: 1234)}** ubicadas en `specs/active/{ID}-*/tasks.md` e inicia la implementación orquestada con TDD.

## Instrucciones

1. Busca las tareas con glob `specs/active/${input:workItemId}-*/tasks.md`.
2. Verifica que el `status` sea `aprobadas` antes de continuar.
3. Lee también la especificación y el plan de la misma carpeta para contexto completo.
4. Ejecuta el flujo TDD completo: Red → Green → Refactor → Coverage → Docs.
5. Al finalizar, mueve la carpeta a `specs/completed/{ID}-{feature-name}/`.
