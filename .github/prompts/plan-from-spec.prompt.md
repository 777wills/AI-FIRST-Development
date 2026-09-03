---
name: plan-from-spec
description: Crea un plan de implementación a partir de una especificación aprobada.
argument-hint: "ID del Work Item — ej: 1234"
agent: Plan Builder
---

# Plan desde Especificación Aprobada

Lee la especificación aprobada del Work Item **${input:workItemId:ID del Work Item (solo el número, ej: 1234)}** ubicada en `specs/active/{ID}-*/specification.md` y crea el plan de implementación detallado.

## Instrucciones

1. Busca la especificación con glob `specs/active/${input:workItemId}-*/specification.md`.
2. Verifica que el `status` sea `aprobada` antes de continuar.
3. Usa la plantilla de plan en [plan-template.md](../../specs/templates/plan-template.md).
4. Explora el codebase para identificar patrones reutilizables.
5. Guarda el plan en `specs/active/{ID}-{feature-name}/plan.md`.
