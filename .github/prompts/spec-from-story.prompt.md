---
name: spec-from-story
description: Crea una especificación técnica a partir de una historia de usuario de Azure DevOps.
argument-hint: "{ID} \"historia de usuario\" — ej: 1234 \"Como usuario quiero...\""
agent: Spec Builder
---

# Especificación desde Historia de Usuario

Crea la especificación técnica para el Work Item con ID **${input:workItemId:ID del Work Item en Azure DevOps (solo el número, ej: 1234)}**.

## Historia de Usuario

${input:userStory:Pega aquí la historia de usuario de Azure DevOps (título, descripción, criterios de aceptación)}

## Instrucciones

1. Usa el ID proporcionado para crear la carpeta `specs/active/{ID}-{feature-name}/`.
2. Usa la plantilla de especificación en [specification-template.md](../../specs/templates/specification-template.md).
3. Explora el codebase para entender las entidades, endpoints y patrones existentes.
4. Hazme preguntas sobre cualquier ambigüedad antes de completar la spec.
5. La especificación debe quedar almacenada en `specs/active/{ID}-{feature-name}/specification.md`.
