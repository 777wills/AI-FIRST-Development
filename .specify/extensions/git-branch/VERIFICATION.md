# ✅ Verificación de Configuración - Git Branch Extension

## Estado de la Configuración

### Archivos Creados

✅ `.specify/extensions/git-branch/extension.yml` - Definición de la extensión
✅ `.specify/extensions/git-branch/commands/create-branch.md` - Documentación del comando
✅ `.specify/extensions/git-branch/scripts/create-git-branch.ps1` - Script PowerShell
✅ `.specify/extensions/git-branch/README.md` - Documentación completa

### Archivos Actualizados

✅ `.specify/extensions.yml` - Hook `before_specify` agregado
✅ `.specify/extensions/.registry` - Extensión registrada

## Prueba Rápida

Para verificar que todo funciona correctamente:

### 1. Verificar que Git está disponible

```powershell
git --version
```

Debería mostrar la versión de git instalada.

### 2. Verificar el estado actual

```powershell
git status
git branch
```

### 3. Probar el script manualmente (opcional)

```powershell
cd d:\SII\SII_BaseApi
.\.specify\extensions\git-branch\scripts\create-git-branch.ps1 `
    -FeatureDescription "Prueba de creación de rama" `
    -Json `
    -DryRun
```

Esto debería mostrar:
```json
{"BRANCH_NAME":"001-prueba-creacion-rama","FEATURE_NUM":"001","BASE_BRANCH":"main","DRY_RUN":true}
```

### 4. Probar con SpecKit

Ahora puedes usar el flujo completo:

```
/speckit.specify Agregar autenticación con OAuth2
```

El flujo automáticamente:
1. ✅ Detecta cambios sin commit (si los hay)
2. ✅ Crea rama `001-autenticacion-oauth2` (o siguiente número disponible)
3. ✅ Cambia a la nueva rama
4. ✅ Continúa con la especificación

## Comportamiento Esperado

### Si todo está bien:

```
[git-branch] Creating new branch: 001-user-auth
Switched to a new branch '001-user-auth'
[git-branch] ✓ Now on branch: 001-user-auth
```

### Si tienes cambios sin commit:

```
[git-branch] Warning: You have uncommitted changes
Do you want to continue anyway? (y/N)
```

### Si la rama ya existe:

```
Error: Branch '001-user-auth' already exists. Use -AllowExistingBranch to switch to it anyway.
```

## Configuración Actual

### Hook Configurado

```yaml
before_specify:
  - extension: git-branch
    command: git.create-branch
    enabled: true        # ✅ Activo
    optional: false      # ✅ Obligatorio
    priority: 10
```

### Rama Base

Por defecto: `main`

Para cambiar, edita el script:
```powershell
# Línea 14 en create-git-branch.ps1
[string]$BaseBranch = "develop"  # Cambia "main" por tu rama base
```

## Solución Rápida de Problemas

| Problema | Solución |
|----------|----------|
| "Git is not available" | Instalar Git o agregarlo al PATH |
| "Branch already exists" | Usa otro nombre o borra la rama existente |
| "Uncommitted changes" | Commit o stash tus cambios primero |
| Hook no se ejecuta | Verifica que `enabled: true` en extensions.yml |

## Siguiente Paso

Ahora puedes ejecutar:

```
/speckit.specify Crear endpoint de productos con paginación
```

Y automáticamente se creará la rama `001-endpoint-productos-paginacion` (o el siguiente número disponible).

---

**Nota**: Si quieres desactivar temporalmente la creación automática, cambia `enabled: true` a `enabled: false` en `.specify/extensions.yml`.
