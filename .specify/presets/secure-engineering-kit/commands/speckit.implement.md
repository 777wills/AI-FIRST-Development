---
description: Ejecuta el plan de implementación procesando y completando todas las tareas definidas en `tasks.md`.
scripts:
  sh: scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks
  ps: scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
---

## Entrada del usuario

```text
$ARGUMENTS
```

**DEBES** considerar la entrada del usuario antes de continuar (si no está vacía).

## Regla de idioma (neutral/adaptativa)

- Responde en el idioma de la solicitud del usuario.
- Si el idioma no está claro, conserva el idioma predominante del artefacto que estás editando.
- No impongas un idioma global para todo el flujo.

## Verificaciones previas a la ejecución

**Comprobar hooks de extensión (antes de implementar):**
- Verifica si existe `.specify/extensions.yml` en la raíz del proyecto.
- Si existe, léelo y busca entradas bajo la clave `hooks.before_implement`.
- Si el YAML no puede analizarse o es inválido, omite silenciosamente la comprobación de hooks y continúa normalmente.
- Filtra los hooks donde `enabled` sea explícitamente `false`. Los hooks sin el campo `enabled` se consideran habilitados por defecto.
- Para cada hook restante, **no** intentes interpretar ni evaluar las expresiones del campo `condition`:
  - Si el hook no tiene `condition`, o es nulo o vacío, considéralo ejecutable.
  - Si el hook define una `condition` no vacía, omítelo y deja la evaluación al `HookExecutor`.
- Para cada hook ejecutable, muestra lo siguiente según su bandera `optional`:
  - **Hook opcional** (`optional: true`):

    ```text
    ## Hooks de extensión

    **Hook previo opcional**: {extension}
    Comando: `/{command}`
    Descripción: {description}

    Prompt: {prompt}
    Para ejecutarlo: `/{command}`
    ```

  - **Hook obligatorio** (`optional: false`):

    ```text
    ## Hooks de extensión

    **Hook previo automático**: {extension}
    Ejecutando: `/{command}`
    EXECUTE_COMMAND: {command}

    Espera el resultado del hook antes de continuar con el flujo.
    ```

    Después de mostrar el bloque anterior **DEBES** invocar realmente el hook y esperar a que finalice antes de continuar. Ejecútalo de la misma forma en que ejecutarías un comando en esta sesión (la invocación puede diferir del identificador `{command}` mostrado).

- Si no existen hooks registrados o `.specify/extensions.yml` no existe, omítelo silenciosamente.

---

# Flujo de implementación

## 1. Inicialización

Ejecuta `{SCRIPT}` desde la raíz del repositorio y analiza `FEATURE_DIR` y `AVAILABLE_DOCS`.

Todos los caminos deben ser absolutos.

Para argumentos con comillas simples (ej. `I'm Groot`), utiliza el escape apropiado:

```sh
'I'\''m Groot'
```

o comillas dobles cuando sea posible.

---

## 2. Verificar el estado de las checklists

Si existe `FEATURE_DIR/checklists/`:

- Analiza todos los archivos de checklist.
- Para cada uno calcula:
  - Total de ítems (`- [ ]`, `- [x]`, `- [X]`)
  - Ítems completados (`- [x]`, `- [X]`)
  - Ítems pendientes (`- [ ]`)

Genera una tabla como:

```text
| Checklist | Total | Completados | Pendientes | Estado |
|-----------|-------|-------------|------------|--------|
| ux.md | 12 | 12 | 0 | ✓ PASS |
| test.md | 8 | 5 | 3 | ✗ FAIL |
| security.md | 6 | 6 | 0 | ✓ PASS |
```

Estado general:

- **PASS**: ninguna checklist tiene pendientes.
- **FAIL**: existe al menos una checklist incompleta.

Si existe alguna checklist incompleta:

- Muestra la tabla.
- **DETÉN la implementación** y pregunta:

> "Algunas checklists aún están incompletas. ¿Deseas continuar con la implementación de todas formas? (sí/no)"

Espera la respuesta del usuario.

- Si responde **no**, **esperar** o **detener**, finaliza la ejecución.
- Si responde **sí**, **continuar** o equivalente, continúa con el paso 3.

Si todas las checklists están completas:

- Muestra la tabla.
- Continúa automáticamente.

---

## 3. Cargar el contexto de implementación

Lee y analiza:

**Obligatorio**

- `tasks.md`
- `plan.md`

**Si existen**

- `data-model.md`
- `contracts/`
- `research.md`
- `security-context.md`
- `/memory/constitution.md`
- `quickstart.md`

Si `security-context.md` existe, úsalo como fuente principal para controles, REQ y evidencia
de seguridad. Evita releer los perfiles completos (`secure-core.md`, `api.spec.md`,
`mobile.spec.md`, `web.spec.md`) salvo discrepancia o solicitud explícita del usuario.

Si `security-context.md` no existe, solicita regenerarlo con `/specify` antes de continuar.
Solo procede sin ese archivo con autorización explícita del usuario.

Utiliza estos documentos con la siguiente prioridad:

1. `constitution.md` (restricciones obligatorias del repositorio)
2. `security-context.md` (controles y trazabilidad operativa de seguridad)
3. `plan.md` (arquitectura y decisiones técnicas)
4. `tasks.md` (orden y alcance de implementación)
5. Resto de documentación de soporte.

No implementes funcionalidades, comportamientos o cambios que no estén respaldados por estos documentos.

Si detectas contradicciones entre ellos, detén la implementación afectada y solicita aclaración en lugar de asumir una solución.

---

## 4. Verificar la configuración del proyecto

**Obligatorio**

Crear o verificar los archivos *ignore* según la tecnología detectada.

### Detección

- Git → `.gitignore`
- Docker → `.dockerignore`
- ESLint → `.eslintignore` o `ignores`
- Prettier → `.prettierignore`
- npm → `.npmignore` (si aplica)
- Terraform → `.terraformignore`
- Helm → `.helmignore`

Si el archivo existe:

- añade únicamente los patrones críticos faltantes.

Si no existe:

- créalo con los patrones apropiados para la tecnología detectada.

Mantén los patrones originales del Spec Kit.

---

## 5. Analizar `tasks.md`

Extrae:

- fases
- dependencias
- tareas paralelas `[P]`
- archivos afectados
- flujo de ejecución

---

## 6. Ejecutar la implementación

Implementa siguiendo estrictamente el plan de tareas.

Reglas obligatorias:

- Completa una fase antes de comenzar la siguiente.
- Respeta todas las dependencias.
- Las tareas paralelas `[P]` solo pueden ejecutarse en paralelo cuando no modifican los mismos archivos.
- Sigue TDD: escribe o valida las pruebas de cada historia **antes** de implementar su lógica de producción; las pruebas deben fallar antes de que exista implementación.
- Mantén consistencia con la arquitectura y convenciones definidas en `plan.md`.
- Implementa únicamente el alcance definido en `tasks.md`.
- No agregues funcionalidades, refactorizaciones o mejoras no planificadas.
- Si una tarea entra en conflicto con el `constitution`, detén esa implementación y repórtalo en lugar de generar código que incumpla las reglas del repositorio.
- Si una implementación requiere asumir un comportamiento no especificado, solicita aclaración antes de continuar.

---

## 7. Reglas de implementación

Orden de ejecución:

1. Configuración inicial.
2. Pruebas.
3. Implementación principal.
4. Integraciones.
5. Validación y pulido.

Durante toda la implementación:

- Mantén coherencia con el estilo del proyecto.
- Reutiliza componentes existentes antes de crear nuevos.
- Minimiza cambios fuera del alcance de la tarea actual.
- Evita introducir deuda técnica innecesaria.

---

## 8. Seguimiento del progreso y manejo de errores

Después de completar cada tarea:

- informa el progreso.

Si falla una tarea secuencial:

- detén la ejecución.

Si fallan tareas paralelas:

- continúa con las exitosas.
- reporta claramente las fallidas.

Siempre:

- proporciona contexto suficiente para depuración.
- sugiere los siguientes pasos cuando no sea posible continuar.

**IMPORTANTE**

Cada tarea completada debe marcarse como `[X]` en `tasks.md`.

---

## 9. Validación final

Verifica que:

- todas las tareas requeridas fueron completadas;
- la implementación coincide con la especificación funcional;
- se respetó el plan técnico;
- las pruebas pasan y la cobertura de líneas, ramas y funciones alcanza los objetivos definidos en la sección "Estrategia de Pruebas" de `plan.md`;
- cada REQ-* de seguridad crítico o alto tiene al menos una prueba automatizada que demuestra su cumplimiento;
- no existen cambios fuera del alcance definido.

**Importante**

El código generado debe considerarse una implementación propuesta.

Su incorporación al repositorio requiere completar el proceso de validación definido por el proyecto (pruebas, revisión de código y demás verificaciones establecidas en el `constitution`).

Si `tasks.md` está incompleto o no existe, sugiere ejecutar `__SPECKIT_COMMAND_TASKS__` para regenerarlo.

---

# Hooks obligatorios posteriores a la implementación

**DEBES** completar esta sección antes de informar que la implementación finalizó.

Verifica si existe `.specify/extensions.yml`.

Si no existe, o no hay hooks bajo `hooks.after_implement`, continúa directamente al reporte final.

Si existe:

- léelo;
- filtra hooks deshabilitados;
- no evalúes condiciones;
- ejecuta únicamente los hooks obligatorios.

Para cada hook:

### Hook obligatorio

```text
## Hooks de extensión

**Hook automático**: {extension}
Ejecutando: `/{command}`
EXECUTE_COMMAND: {command}
```

Después de mostrar el bloque, **DEBES** ejecutar realmente el hook y esperar a que termine.

### Hook opcional

```text
## Hooks de extensión

**Hook opcional**: {extension}
Comando: `/{command}`
Descripción: {description}

Prompt: {prompt}
Para ejecutarlo: `/{command}`
```

---

# Reporte final

Informa el estado final y resume el trabajo realizado.

---

# Finalizado cuando

- [ ] Todas las tareas de `tasks.md` están marcadas como `[X]`.
- [ ] La implementación fue validada contra la especificación, el plan y los objetivos de cobertura de pruebas definidos en `plan.md`.
- [ ] Cada REQ-* crítico o alto tiene al menos una prueba automatizada que demuestra su cumplimiento.
- [ ] Los hooks posteriores fueron ejecutados u omitidos según las reglas.
- [ ] El usuario recibió un resumen final de la implementación.