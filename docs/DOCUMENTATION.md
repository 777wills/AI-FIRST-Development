# 📚 Índice de Documentación - Olimpia API .NET 10

Guía completa para navegar la documentación del proyecto Olimpia.

---

## 🎯 Comienza Aquí

### Nuevo Desarrollador
1. **[Inicio Rápido](#inicio-rápido)** - 5 minutos
2. **[ARCHITECTURE.md](ARCHITECTURE.md)** - Entender la estructura
3. **[PATTERNS.md](PATTERNS.md)** - Cómo se codifica aquí
4. **[DATA_ACCESS.md](DATA_ACCESS.md)** - Acceso a datos

### Tech Lead / Arquitecto
1. **[ARCHITECTURE.md](ARCHITECTURE.md)** - Visión general
2. **[PATTERNS.md](PATTERNS.md)** - Patrones y decisiones
3. **[RESILIENCE.md](RESILIENCE.md)** - Confiabilidad

### DevOps / SRE
1. **[DEPLOYMENT.md](DEPLOYMENT.md)** - Docker y Kubernetes
2. **[CONFIGURATION.md](CONFIGURATION.md)** - Variables de entorno
3. **[README.md](README.md)** - Stack y features

### QA / Testing
1. **[TESTING.md](TESTING.md)** - Estrategia de tests
2. **[ARCHITECTURE.md](ARCHITECTURE.md)** - Estructura para validación

### Code Reviewer
1. **[PATTERNS.md](PATTERNS.md)** - Estándares de código
2. **[.github/copilot-instructions.md](.github/copilot-instructions.md)** - Reglas obligatorias

---

## 📖 Documentos Disponibles

### 🏗️ Arquitectura y Diseño

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[ARCHITECTURE.md](ARCHITECTURE.md)** | Capas, dependencias, estructura del proyecto, regla global:: | Todos | 20 min |
| **[PATTERNS.md](PATTERNS.md)** | CQRS, Repository, Unit of Work, Decorators | Developers | 25 min |
| **[.github/copilot-instructions.md](.github/copilot-instructions.md)** | Reglas de codificación, stack tecnológico | Developers | 15 min |

### 💾 Persistencia y Datos

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[DATA_ACCESS.md](DATA_ACCESS.md)** | Dapper, SqlKata, GenericRepository, SP, Views | Developers | 30 min || **[PAGINATION.md](PAGINATION.md)** | Paginación, filtrado, ordenamiento y envelope reutilizable | Developers | 20 min || **[CACHING.md](CACHING.md)** | Redis, IDistributedCache, estrategias de expiración | Developers | 20 min |

### 🔐 Seguridad e Integración

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[AUTHENTICATION.md](AUTHENTICATION.md)** | JWT, OpenIddict, Resource Server | Developers | 25 min |
| **[HTTP_CLIENTS.md](HTTP_CLIENTS.md)** | Token propagation, Polly, reintentos | Developers | 25 min |

### 🛡️ Confiabilidad

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[RESILIENCE.md](RESILIENCE.md)** | Polly, Circuit Breaker, Timeout, Fallback | Developers, Architects | 20 min |

### ⚙️ Operaciones

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[CONFIGURATION.md](CONFIGURATION.md)** | Variables de entorno, secretos, appsettings | DevOps, Developers | 20 min |
| **[DEPLOYMENT.md](DEPLOYMENT.md)** | Docker, Docker Compose, Kubernetes | DevOps, SRE | 30 min |

### 🧪 Testing

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[TESTING.md](TESTING.md)** | MSTest, Moq, FluentAssertions, fixtures | Developers, QA | 25 min |

### 📋 Referencia

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[README.md](README.md)** | Visión general, features, quick start | Todos | 15 min |

### 📊 Logging y Monitoreo

| Documento | Propósito | Audiencia | Lectura |
|-----------|----------|-----------|---------|
| **[LOGGING_CENTRAL.md](LOGGING_CENTRAL.md)** | LogCentral, tipos de log, encolado offline, middlewares | Developers, DevOps | 30 min |

---

## 🔍 Búsqueda por Tema

### Entender Cómo Funciona

- **Control de flujo de una solicitud HTTP**
  - Leer: [ARCHITECTURE.md#flujo-de-una-solicitud-http](ARCHITECTURE.md#flujo-de-una-solicitud-http)
  - Tiempo: 5 min

- **CQRS y Mediator**
  - Leer: [PATTERNS.md#1-cqrs-con-cortex-mediator](PATTERNS.md#1-cqrs-con-cortex-mediator)
  - Tiempo: 10 min

- **Acceso a Datos**
  - Leer: [DATA_ACCESS.md](DATA_ACCESS.md)
  - Tiempo: 30 min

- **Paginación, Filtrado y Ordenamiento**
  - Leer: [PAGINATION.md](PAGINATION.md)
  - Tiempo: 20 min

- **Autenticación y Autorización**
  - Leer: [AUTHENTICATION.md](AUTHENTICATION.md)
  - Tiempo: 20 min

- **Logging y Monitoreo**
  - Leer: [LOGGING_CENTRAL.md](LOGGING_CENTRAL.md)
  - Tiempo: 10 min

### Escribir Código

- **Crear nuevo Handler (Command)**
  - Leer: [PATTERNS.md#11-commands-escritura-de-datos](PATTERNS.md#11-commands-escritura-de-datos)
  - Tiempo: 10 min

- **Crear nuevo Repository**
  - Leer: [PATTERNS.md#2-repository-pattern-con-genericrepository](PATTERNS.md#2-repository-pattern-con-genericrepository)
  - Tiempo: 10 min

- **Agregar Paginación a un Listado**
  - Leer: [PAGINATION.md#7-cómo-agregar-paginación-a-un-nuevo-feature](PAGINATION.md#7-cómo-agregar-paginación-a-un-nuevo-feature)
  - Tiempo: 10 min

- **Llamar API externa**
  - Leer: [HTTP_CLIENTS.md#7-uso-en-handlers](HTTP_CLIENTS.md#7-uso-en-handlers)
  - Tiempo: 10 min

- **Implementar Caché**
  - Leer: [CACHING.md#2-patrón-cache-aside-lazy-loading](CACHING.md#2-patrón-cache-aside-lazy-loading)
  - Tiempo: 10 min

### Testing

- **Test un Handler**
  - Leer: [TESTING.md#3-testing-de-handlers](TESTING.md#3-testing-de-handlers)
  - Tiempo: 15 min

- **Test un Repositorio**
  - Leer: [TESTING.md#4-testing-de-repositorios](TESTING.md#4-testing-de-repositorios)
  - Tiempo: 10 min

- **Test un Validator**
  - Leer: [TESTING.md#5-testing-de-validators](TESTING.md#5-testing-de-validators)
  - Tiempo: 10 min

### Operaciones

- **Configurar para Desarrollo**
  - Leer: [CONFIGURATION.md#2-appsettingsdevelopmentjson](CONFIGURATION.md#2-appsettingsdevelopmentjson)
  - Tiempo: 5 min

- **Configurar para Producción**
  - Leer: [CONFIGURATION.md#3-appsettingsproductionjson](CONFIGURATION.md#3-appsettingsproductionjson)
  - Tiempo: 5 min

- **Desplegar con Docker**
  - Leer: [DEPLOYMENT.md#2-docker-build-y-run](DEPLOYMENT.md#2-docker-build-y-run)
  - Tiempo: 10 min

- **Desplegar con Kubernetes**
  - Leer: [DEPLOYMENT.md#5-kubernetes---manifest](DEPLOYMENT.md#5-kubernetes---manifest)
  - Tiempo: 20 min

---

## 🚦 Matriz por Fase del Proyecto

### 1️⃣ Setup Local (Día 1)

1. [README.md](README.md) - Visión general
2. [ARCHITECTURE.md](ARCHITECTURE.md) - Entender estructura
3. [CONFIGURATION.md#2-appsettingsdevelopmentjson](CONFIGURATION.md#2-appsettingsdevelopmentjson) - Configurar

**Tiempo total:** 30 minutos

### 2️⃣ Aprender a Codificar (Día 1-2)

1. [PATTERNS.md](PATTERNS.md) - Patrones
2. [DATA_ACCESS.md](DATA_ACCESS.md) - Acceso a datos
3. [AUTHENTICATION.md](AUTHENTICATION.md) - Seguridad

**Tiempo total:** 75 minutos

### 3️⃣ Implementar Primer Feature (Día 2-3)

1. [PATTERNS.md#11-commands-escritura-de-datos](PATTERNS.md#11-commands-escritura-de-datos) - Crear handler
2. [DATA_ACCESS.md#4-genericrepository](DATA_ACCESS.md#4-genericrepository) - Usar repositorio
3. [TESTING.md#3-testing-de-handlers](TESTING.md#3-testing-de-handlers) - Testear

**Tiempo total:** 45 minutos

### 4️⃣ Integración (Día 3-4)

1. [HTTP_CLIENTS.md](HTTP_CLIENTS.md) - APIs externas
2. [CACHING.md](CACHING.md) - Optimización
3. [RESILIENCE.md](RESILIENCE.md) - Confiabilidad

**Tiempo total:** 60 minutos

### 5️⃣ Deployment (Semana 2)

1. [DEPLOYMENT.md](DEPLOYMENT.md) - Docker/K8s
2. [CONFIGURATION.md](CONFIGURATION.md) - Configuración por entorno
3. [.github/copilot-instructions.md](.github/copilot-instructions.md) - Revisión final

**Tiempo total:** 90 minutos

---

## ✅ Checklists Rápidos

### Crear nuevo Endpoint

- [ ] Leer [PATTERNS.md#11-commands-escritura-de-datos](PATTERNS.md#11-commands-escritura-de-datos)
- [ ] Crear `XxxCommand` o `XxxQuery` record
- [ ] Crear Handler implementando `ICommandHandler<>` o `IQueryHandler<>`
- [ ] Crear Validator heredando de `AbstractValidator<>`
- [ ] Registrar validator en `DependencyInjection.cs`
- [ ] Crear test en [TESTING.md#3-testing-de-handlers](TESTING.md#3-testing-de-handlers)
- [ ] Crear Controller endpoint llamando `_mediator.SendAsync()`
- [ ] Documentar en Swagger
- [ ] Verificar autenticación/autorización ([AUTHENTICATION.md](AUTHENTICATION.md))

### Crear nuevo Repositorio

- [ ] Leer [PATTERNS.md#2-repository-pattern-con-genericrepository](PATTERNS.md#2-repository-pattern-con-genericrepository)
- [ ] Crear `IXxxRepository : IGenericRepository<T>` en Domain
- [ ] Crear `XxxRepository : GenericRepository<T>` en Infrastructure
- [ ] Métodos específicos usan `global::` ([ARCHITECTURE.md#regla-global-en-la-arquitectura](ARCHITECTURE.md#regla-global-en-la-arquitectura))
- [ ] Verificar auto-registro en DI
- [ ] Crear test en [TESTING.md#4-testing-de-repositorios](TESTING.md#4-testing-de-repositorios)
- [ ] Usar en handlers sin SQL crudo

### Integración con API Externa

- [ ] Leer [HTTP_CLIENTS.md](HTTP_CLIENTS.md)
- [ ] Definir método en `IExternalApiClient`
- [ ] Inyectar en handler
- [ ] Usar `await _externalApiClient.GetAsync<T>(...)`
- [ ] Token se propaga automáticamente
- [ ] Reintentos automáticos con Polly
- [ ] Test con mocks de `IExternalApiClient`

### Implementar Caché

- [ ] Leer [CACHING.md](CACHING.md)
- [ ] En Query handler:
  - [ ] `await _cache.GetStringAsync(key)` primero
  - [ ] Si null, obtener de BD
  - [ ] `await _cache.SetStringAsync(key, json, options)`
- [ ] En Command handler:
  - [ ] `await _cache.RemoveAsync(key)` después de actualizar
- [ ] Testear cache hit y miss ([TESTING.md](TESTING.md))

### Deploy en Producción

- [ ] Leer [DEPLOYMENT.md](DEPLOYMENT.md)
- [ ] Actualizar [CONFIGURATION.md#3-appsettingsproductionjson](CONFIGURATION.md#3-appsettingsproductionjson)
- [ ] Generar imagen Docker
- [ ] Verificar health checks
- [ ] Aplicar manifests K8s
- [ ] Verificar logs en LogCentral
- [ ] Monitoreo y alertas configuradas

---

## 🎓 Recursos Externos

### Stack Tecnológico

- **[.NET 10 Docs](https://learn.microsoft.com/dotnet/)**
- **[Cortex.Mediator GitHub](https://github.com/Cortex-Cloud/cortex.mediator)**
- **[FluentValidation](https://fluentvalidation.net/)**
- **[Dapper](https://github.com/DapperLib/Dapper)**
- **[SqlKata](https://sqlkata.com/)**
- **[Polly Documentation](https://github.com/App-vNext/Polly)**
- **[OpenIddict Documentation](https://documentation.openiddict.com/)**

### Patrones

- **[Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)**
- **[CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)**
- **[Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)**
- **[Circuit Breaker](https://martinfowler.com/bliki/CircuitBreaker.html)**

---

## 📞 Preguntas Frecuentes

**P: ¿Por qué no usar Entity Framework?**
R: Ver [ARCHITECTURE.md#decisiones-arquitectónicas](ARCHITECTURE.md#decisiones-arquitectónicas)

**P: ¿Cómo agregar un nuevo repositorio sin modificar DI?**
R: Auto-registro por reflexión - ver [PATTERNS.md#auto-registro-de-repositorios](PATTERNS.md#auto-registro-de-repositorios)

**P: ¿Dónde van los secretos en producción?**
R: Variables de entorno o Key Vault - ver [CONFIGURATION.md#8-configuración-segura-en-producción](CONFIGURATION.md#8-configuración-segura-en-producción)

**P: ¿Cómo testeo un handler?**
R: MSTest + Moq + FluentAssertions - ver [TESTING.md#3-testing-de-handlers](TESTING.md#3-testing-de-handlers)

---

## 🔗 Próximos Pasos

- Explorar la documentación según tu rol (arriba)
- Clonar el repositorio: `git clone https://...`
- Consultar README.md para setup local
- Hacer tu primer pull request 🚀

---

## 📝 Historial de Cambios

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | 2024-01-15 | Documentación inicial completada |

---

**¿Dudas? Abre un issue en el repositorio o contacta al equipo de arquitectura.**

**¡Bienvenido a Olimpia!** 🎉
