# Secure Engineering Kit Preset

Preset para Spec Kit orientado a desarrollo seguro con reglas de gobierno,
trazabilidad de requisitos de seguridad y alineacion con marcos como OWASP,
WSTG y MASVS.

## Resumen Rapido

Este preset reemplaza los comandos y plantillas base de Spec Kit para que,
desde la etapa de especificacion hasta la implementacion, se incluyan
controles de seguridad, evidencia y validaciones minimas.

Se creo para estandarizar practicas de seguridad en equipos que ya usan
Spec Kit, evitando que la seguridad quede como una actividad separada o
tardia.

## Que Incluye

- Plantillas de constitucion, especificacion, plan y tareas con enfoque seguro.
- Comandos `speckit.constitution`, `speckit.specify`, `speckit.plan`,
  `speckit.tasks` y `speckit.implement` adaptados al preset.
- Memorias de seguridad por tipo de componente (`secure-core`, `api`, `web`,
  `mobile`) para guiar requerimientos y evidencia.

## Objetivo del Preset

- Integrar seguridad desde el inicio del ciclo de vida del feature.
- Mantener consistencia entre especificacion, plan y tareas.
- Facilitar auditoria y trazabilidad de decisiones de seguridad.

## Uso Basico

1. Instalar el preset en modo local.
2. Ejecutar flujo normal de Spec Kit (`specify`, `plan`, `tasks`, `implement`).
3. Verificar que cada feature genere y mantenga su contexto de seguridad.

## Licencia

MIT
