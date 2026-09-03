# 🚀 Deployment - Docker, Kubernetes y Podman

Documentación de empaquetamiento y despliegue en contenedores.

---

## 1. Dockerfile

```dockerfile
# Olimpia.Api/Dockerfile

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Olimpia.Api/Olimpia.Api.csproj", "src/Olimpia.Api/"]
COPY ["src/Olimpia.Application/Olimpia.Application.csproj", "src/Olimpia.Application/"]
COPY ["src/Olimpia.Infrastructure/Olimpia.Infrastructure.csproj", "src/Olimpia.Infrastructure/"]
COPY ["src/Olimpia.Infrastructure.Logging/Olimpia.Infrastructure.Logging.csproj", "src/Olimpia.Infrastructure.Logging/"]
COPY ["src/Olimpia.Domain/Olimpia.Domain.csproj", "src/Olimpia.Domain/"]

# Restaurar dependencias
RUN dotnet restore "src/Olimpia.Api/Olimpia.Api.csproj"

# Copiar código fuente
COPY . .

# Build
WORKDIR "/src/src/Olimpia.Api"
RUN dotnet build "Olimpia.Api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "Olimpia.Api.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app

# Crear directorio para logs
RUN mkdir -p /var/log/olimpia-template

# Copiar artefactos publicados
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

# Exponer puerto
EXPOSE 8080

# Ejecutar
ENTRYPOINT ["dotnet", "Olimpia.Api.dll"]
```

---

## 2. Docker Build y Run

### Build

```bash
# Construir imagen
docker build -t olimpia-template:latest .

# Con etiqueta
docker build -t olimpia-template:1.0.0 .
docker build -t olimpia-template:1.0.0 -t olimpia-template:latest .
```

### Run - Desarrollo (Sin LogCentral)

```bash
docker run -d \
  --name olimpia-template \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "Jwt__Authority=http://localhost:5001" \
  -e "Jwt__Audience=olimpia-template" \
  -e "Jwt__RequireHttpsMetadata=false" \
  -e "ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OlimpiaDb;User Id=sa;Password=Pass123!;TrustServerCertificate=True;" \
  -e "Logging__LogCentral__Enabled=false" \
  -e "RedisCache__Enabled=false" \
  olimpia-template:latest
```

### Run - Producción (Con LogCentral)

```bash
docker run -d \
  --name olimpia-template-prod \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "Jwt__Authority=https://identity.production.com" \
  -e "Jwt__Audience=olimpia-template-prod" \
  -e "Jwt__RequireHttpsMetadata=true" \
  -e "ConnectionStrings__DefaultConnection=Server=sqlserver.prod;Database=OlimpiaDb;User Id=sa;Password=SECRETPRODUCTION;TrustServerCertificate=False;" \
  -e "Logging__CustomLogger__MinimumLevel=Information" \
  -e "Logging__CustomLogger__Path=/var/log/olimpia-template" \
  -e "Logging__LogCentral__Enabled=true" \
  -e "Logging__LogCentral__BaseUrl=https://logcentral.production.com" \
  -e "Logging__LogCentral__Timeout=30000" \
  -e "RedisCache__Enabled=true" \
  -e "RedisCache__ConnectionString=redis.prod:6379" \
  -v /var/log/olimpia-template:/var/log/olimpia-template \
  olimpia-template:latest
```

### Ver Logs

```bash
# Logs de contenedor
docker logs -f olimpia-template

# Logs desde volumen
docker exec -it olimpia-template tail -f /var/log/olimpia-template/2024-01-15.jsonl
```

---

## 3. Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: olimpia-template
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Jwt__Authority=http://identity:5001
      - Jwt__Audience=olimpia-template
      - Jwt__RequireHttpsMetadata=false
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OlimpiaDb;User Id=sa;Password=Pass@123!;TrustServerCertificate=True;
      - Logging__LogCentral__Enabled=false
      - RedisCache__Enabled=true
      - RedisCache__ConnectionString=redis:6379
      - ExternalApis__CatalogoService__BaseUrl=http://catalogo:8080
    depends_on:
      sqlserver:
        condition: service_healthy
      redis:
        condition: service_started
    networks:
      - olimpia-template-network
    restart: unless-stopped

  sqlserver:
    image: mcr.microsoft.com/mssql/server:latest
    container_name: olimpia-template-sqlserver
    environment:
      SA_PASSWORD: Pass@123!
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql/data
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Pass@123! -Q "SELECT 1"
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - olimpia-template-network
    restart: unless-stopped

  redis:
    image: redis:latest
    container_name: olimpia-template-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - olimpia-template-network
    restart: unless-stopped

  identity:
    image: olimpia-template-identity:latest  # OpenIddict Authorization Server
    container_name: olimpia-template-identity
    ports:
      - "5001:5001"
    environment:
      - ASPNETCORE_URLS=http://+:5001
    networks:
      - olimpia-template-network
    restart: unless-stopped

volumes:
  sqlserver-data:
  redis-data:

networks:
  olimpia-template-network:
    driver: bridge
```

### Ejecutar Docker Compose

```bash
# Up (crear y ejecutar)
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Down (detener y remover)
docker-compose down

# Remover también volúmenes
docker-compose down -v
```

---

## 4. Podman (Alternativa a Docker)

Podman es compatible con Docker pero sin daemon:

```bash
# Build
podman build -t olimpia-template:latest .

# Run
podman run -d \
  --name olimpia-template \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__DefaultConnection=..." \
  olimpia-template:latest

# Ver logs
podman logs -f olimpia-template

# Pod (grupo de contenedores)
podman pod create --name olimpia-template
podman run --pod olimpia-template mcr.microsoft.com/mssql/server:latest
podman run --pod olimpia-template olimpia-template:latest

# Generar Kubernetes YAML desde Podman
podman generate kube olimpia-template > olimpia-template.yml
```

---

## 5. Kubernetes - Manifest

```yaml
# k8s/api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: olimpia-template
  namespace: default
  labels:
    app: olimpia-template
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: olimpia-template
  template:
    metadata:
      labels:
        app: olimpia-template
    spec:
      containers:
      - name: api
        image: olimpia-template:1.0.0
        imagePullPolicy: IfNotPresent
        ports:
        - containerPort: 8080
          name: http
        
        # Variables de entorno desde ConfigMap y Secret
        envFrom:
        - configMapRef:
            name: olimpia-template-config
        - secretRef:
            name: olimpia-template-secrets
        
        # Recursos
        resources:
          requests:
            cpu: 250m
            memory: 512Mi
          limits:
            cpu: 500m
            memory: 1Gi
        
        # Health checks
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        
        # Volumen para logs
        volumeMounts:
        - name: logs
          mountPath: /var/log/olimpia-template
      
      volumes:
      - name: logs
        emptyDir: {}
      
      # Tolerancias y afinity
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - olimpia-template
              topologyKey: kubernetes.io/hostname

---
# Service
apiVersion: v1
kind: Service
metadata:
  name: olimpia-template-svc
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
  selector:
    app: olimpia-template

---
# Ingress
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: olimpia-template-ingress
spec:
  ingressClassName: nginx
  rules:
  - host: api.production.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: olimpia-template-svc
            port:
              number: 80
  tls:
  - hosts:
    - api.production.com
    secretName: olimpia-template-tls-cert
```

### ConfigMap y Secret

```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: olimpia-template-config
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Jwt__Authority: "https://identity.production.com"
  Jwt__Audience: "olimpia-template-prod"
  Jwt__RequireHttpsMetadata: "true"
  Logging__CustomLogger__MinimumLevel: "Information"
  Logging__LogCentral__Enabled: "true"
  Logging__LogCentral__BaseUrl: "https://logcentral.production.com"
  RedisCache__Enabled: "true"
  RedisCache__ConnectionString: "redis-cluster:6379"
  ExternalApis__CatalogoService__BaseUrl: "https://catalogo.production.com"

---
# k8s/secret.yaml (generado desde archivo)
apiVersion: v1
kind: Secret
metadata:
  name: olimpia-template-secrets
type: Opaque
stringData:
  ConnectionStrings__DefaultConnection: "Server=sql-server;Database=OlimpiaDb;User Id=sa;Password=SECRETO;TrustServerCertificate=False;"
  Logging__LogCentral__BaseUrl: "https://logcentral.production.com"
  RedisCache__ConnectionString: "redis-cluster:6379"
```

### Desplegar en Kubernetes

```bash
# Aplicar manifests
kubectl apply -f k8s/

# Verificar deployment
kubectl get deployments
kubectl get pods
kubectl get services
kubectl get ingress

# Ver logs
kubectl logs -f deployment/olimpia-template

# Escalar
kubectl scale deployment olimpia-template --replicas=5

# Rollback
kubectl rollout history deployment/olimpia-template
kubectl rollout undo deployment/olimpia-template --to-revision=2

# Delete
kubectl delete -f k8s/
```

---

## 6. Health Checks

```csharp
// Olimpia.Api/Program.cs
var app = builder.Build();

// Health check básico
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .WithOpenApi()
    .AllowAnonymous();

// Health check detallado
app.MapGet("/health/ready", async () =>
{
    var checks = new Dictionary<string, bool>();
    
    // Verificar BD
    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        checks["database"] = true;
    }
    catch
    {
        checks["database"] = false;
    }
    
    // Verificar Redis (si habilitado)
    if (cacheConfig.GetValue<bool>("Enabled"))
    {
        try
        {
            var cache = app.Services.GetRequiredService<IDistributedCache>();
            await cache.GetStringAsync("health-check");
            checks["redis"] = true;
        }
        catch
        {
            checks["redis"] = false;
        }
    }
    
    var allHealthy = checks.Values.All(v => v);
    return Results.Ok(new { status = allHealthy ? "ready" : "degraded", checks });
})
.WithName("HealthReady")
.WithOpenApi()
.AllowAnonymous();

app.Run();
```

---

## 7. Tagging y Versionado

```bash
# Versionado semántico
docker build -t olimpia-template:1.0.0 .
docker tag olimpia-template:1.0.0 olimpia-template:latest
docker tag olimpia-template:1.0.0 olimpia-template:1.0
docker tag olimpia-template:1.0.0 olimpia-template:1

# Registrar (Docker Hub / Azure Container Registry)
docker tag olimpia-template:1.0.0 myregistry.azurecr.io/olimpia-template:1.0.0
docker push myregistry.azurecr.io/olimpia-template:1.0.0
```

---

## 8. Security Best Practices

| Recomendación | Implementación |
|---------------|----------------|
| ✅ Non-root user | `USER app` en Dockerfile |
| ✅ Read-only filesystem | `readOnlyRootFilesystem: true` en K8s |
| ✅ No secrets en logs | Redactar passwords en CustomLogger |
| ✅ Network policies | `NetworkPolicy` en K8s |
| ✅ Resource limits | `resources.limits` en K8s |
| ✅ HTTPS/TLS | Ingress con certificados |
| ✅ Image scanning | `docker scan olimpia-template` |
| ❌ Run as root | Siempre usar usuario no-root |

---

## 9. Monitoreo y Alertas

```yaml
# Prometheus ServiceMonitor
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: olimpia-template
spec:
  selector:
    matchLabels:
      app: olimpia-template
  endpoints:
  - port: http
    interval: 30s
    path: /metrics
```

---

## Próximos Pasos

- **[CONFIGURATION.md](CONFIGURATION.md)** - Variables de entorno
- **[LOGGING_CENTRAL.md](LOGGING_CENTRAL.md)** - Monitoreo con LogCentral
