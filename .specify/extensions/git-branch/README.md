# Git Branch Extension for SpecKit

Esta extensión crea automáticamente una rama de git cuando se inicia el flujo de especificación con `/speckit.specify`.

## Características

- ✅ **Creación automática de ramas** antes de la especificación
- ✅ **Nombres inteligentes** basados en la descripción de la feature
- ✅ **Numeración secuencial** o basada en timestamp
- ✅ **Validación de ramas existentes**
- ✅ **Detección de cambios sin commit**

## Flujo de Trabajo

1. Usuario ejecuta: `/speckit.specify "Agregar autenticación de usuarios"`
2. Hook `before_specify` se activa automáticamente
3. Script genera nombre de rama: `003-user-auth` (o `20260820-143022-user-auth`)
4. Script crea y cambia a la nueva rama: `git checkout -b 003-user-auth`
5. SpecKit continúa con la especificación normalmente

## Configuración

La extensión está configurada en [.specify/extensions.yml](../../extensions.yml):

```yaml
hooks:
  before_specify:
  - extension: git-branch
    command: git.create-branch
    enabled: true
    optional: false
    priority: 10
```

### Opciones

- `enabled: true` - La extensión está activa
- `optional: false` - Es obligatoria (falla si no puede crear la rama)
- `priority: 10` - Prioridad de ejecución (menor = primero)

## Nombres de Rama

El script genera nombres de rama siguiendo estas reglas:

### Formato
- **Secuencial**: `NNN-nombre-corto` (ej: `003-user-auth`)
- **Timestamp**: `YYYYMMDD-HHMMSS-nombre-corto` (ej: `20260820-143022-user-auth`)

### Generación del Nombre
1. Extrae palabras significativas de la descripción
2. Filtra palabras vacías (stop words)
3. Toma las 3-4 palabras más importantes
4. Une con guiones y convierte a minúsculas
5. Preserva acrónimos (OAuth2, API, etc.)

### Ejemplos

| Descripción | Rama Generada |
|-------------|---------------|
| "Agregar autenticación de usuarios" | `003-autenticación-usuarios` |
| "Implementar integración OAuth2 para API" | `004-oauth2-api-integration` |
| "Crear dashboard de analíticas" | `005-dashboard-analíticas` |
| "Corregir bug de timeout en pagos" | `006-corregir-timeout-pagos` |

## Detección de Cambios

Si tienes cambios sin commit, el script:
1. Muestra una advertencia
2. Pregunta si deseas continuar
3. Permite cancelar o continuar

```
[git-branch] Warning: You have uncommitted changes
Do you want to continue anyway? (y/N)
```

## Rama Base

Por defecto, las nuevas ramas se crean desde `main`. Si necesitas cambiar esto:

```powershell
# En el script, cambia el parámetro $BaseBranch
[string]$BaseBranch = "develop"
```

## Solución de Problemas

### Error: "Branch already exists"
La rama ya existe en el repositorio. Opciones:
- Usa otro nombre de feature
- Ejecuta el script con `-AllowExistingBranch` para cambiar a la rama existente

### Error: "Git is not available"
Git no está instalado o no está en el PATH. Instala Git y reinicia VS Code.

### Error: "Failed to create branch"
Posibles causas:
- No tienes permisos en el repositorio
- Estás en un estado de git inválido (rebase, merge en progreso)
- El nombre de la rama es inválido

## Desactivar la Extensión

Si quieres desactivar la creación automática de ramas:

En [.specify/extensions.yml](../../extensions.yml):
```yaml
hooks:
  before_specify:
  - extension: git-branch
    command: git.create-branch
    enabled: false  # Cambiar a false
```

O hacer el hook opcional:
```yaml
hooks:
  before_specify:
  - extension: git-branch
    command: git.create-branch
    optional: true  # Preguntará antes de ejecutar
```

## Integración con SpecKit

El script retorna JSON que SpecKit usa para el resto del flujo:

```json
{
  "BRANCH_NAME": "003-user-auth",
  "FEATURE_NUM": "003",
  "BASE_BRANCH": "main"
}
```

Estos valores están disponibles para comandos posteriores como `/speckit.plan` y `/speckit.tasks`.

## Archivos de la Extensión

```
.specify/extensions/git-branch/
├── extension.yml              # Definición de la extensión
├── README.md                  # Esta documentación
├── commands/
│   └── create-branch.md      # Documentación del comando
└── scripts/
    └── create-git-branch.ps1 # Script PowerShell principal
```
