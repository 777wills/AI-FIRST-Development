---
name: clean-arch-validation
description: "Valida que el código siga las reglas de Clean Architecture del proyecto Olimpia. Usar cuando se creen o modifiquen archivos para verificar que las dependencias entre capas sean correctas."
---

## Checklist por Capa

### Domain (`src/Olimpia.Domain/`)

1. ¿El archivo tiene `using` de namespaces fuera de `Olimpia.Domain`? → **FALLO**
2. ¿Referencia paquetes NuGet? → **FALLO** (Domain es puro C#)
3. ¿Las interfaces siguen la convención `I{Nombre}Repository`? → Verificar
4. ¿Las entidades heredan de `BaseEntity`? → Verificar
5. ¿Las entidades son `sealed`? → Verificar (excepto `BaseEntity` que es `abstract`)

### Application (`src/Olimpia.Application/`)

1. ¿El archivo referencia `Olimpia.Infrastructure`? → **FALLO**
2. ¿El archivo referencia `Olimpia.Api`? → **FALLO**
3. ¿Los Commands implementan `ICommand<T>`? → Verificar
4. ¿Los Queries implementan `IQuery<T>`? → Verificar
5. ¿Los Handlers son `sealed`? → Verificar
6. ¿Los Validators son `sealed`? → Verificar

### Infrastructure (`src/Olimpia.Infrastructure/`)

1. ¿El archivo usa tipos de `Olimpia.Application` fuera de DI? → **FALLO**
2. ¿El archivo referencia `Olimpia.Api`? → **FALLO**
3. ¿Los repositorios implementan las interfaces de Domain? → Verificar
4. ¿Los repositorios concretos son `sealed`? → Verificar

### Api (`src/Olimpia.Api/`)

1. ¿Los controllers heredan de `ApiController`? → Verificar
2. ¿Usan `IMediator` para despachar commands/queries? → Verificar
3. ¿No tienen lógica de negocio directa? → Verificar
4. ¿Los controllers son `sealed`? → Verificar

## Verificación

```bash
dotnet build
```

Si compila sin errores, las dependencias de proyecto son correctas a nivel de `.csproj`. Esta skill valida las dependencias a nivel de `using` y convenciones de código.
