---
name: create-pr
description: Redacta y publica el Pull Request en Azure DevOps con revisión previa del developer.
argument-hint: "ID del Work Item — ej: 1234"
agent: PR Builder
---

# Crear Pull Request

Prepara y publica el Pull Request del Work Item **${input:workItemId:ID del Work Item (solo el número, ej: 1234)}** en Azure DevOps.

## Instrucciones

1. Busca la carpeta del feature con glob `specs/active/${input:workItemId}-*/`.
2. Lee `specification.md` y `tasks.md` para construir el contexto del PR.
3. Consulta el historial de commits del branch actual (`git log --oneline origin/main..HEAD`).
4. Redacta el borrador del PR (título, descripción, work item vinculado, checklist).
5. **Muestra el borrador al developer y espera aprobación explícita antes de publicar.**
6. Solo tras "Aprobado ✅": ejecuta `az repos pr create` con el contenido aprobado.
7. Mueve `specs/active/${input:workItemId}-*/` → `specs/completed/` al finalizar.
