# 🔐 Autenticación y Autorización - Olimpia API

Documentación de **JWT Bearer** multi-proveedor: OIDC (OpenIddict / Keycloak) + Simétrico (HS256).

---

## Modelo: Resource Server Multi-Proveedor

Olimpia implementa el modelo **Resource Server** con soporte para múltiples proveedores JWT simultáneos:

- ❌ **No emite tokens** — eso lo hace el proveedor externo (OpenIddict / Keycloak)
- ✅ **Valida tokens** desde múltiples emisores vía `PolicyScheme + ForwardDefaultSelector`
- ✅ **Implementa políticas de autorización** basadas en scopes

El `ForwardDefaultSelector` inspecciona el claim `iss` del token entrante para determinar
qué esquema usar: si coincide con el issuer de un proveedor simétrico → usa ese esquema;
si no → usa el proveedor OIDC por defecto.

---

## Configuración

### appsettings.json

```json
{
  "Jwt": {
    "Providers": [
      {
        "Name": "OpenIddict",
        "Type": "Oidc",
        "Enabled": true,
        "Authority": "https://localhost:5001",
        "Audience": "olimpia-api",
        "RequireHttpsMetadata": false
      },
      {
        "Name": "Internal",
        "Type": "Symmetric",
        "Enabled": false,
        "Issuer": "olimpia-internal",
        "SigningKey": "<<proveer via Docker Secret o env var Jwt__Providers__1__SigningKey>>"
      }
    ]
  }
}
```

### Clases de Configuración (`Olimpia.Application.Common.Configuration`)

| Clase | Propósito |
|-------|-----------|
| `JwtOptions` | Raíz — lista de `JwtProviderOptions`. |
| `JwtProviderOptions` | Configuración de un proveedor: `Name`, `Type`, `Enabled`, `Authority`, `Audience`, `RequireHttpsMetadata`, `Issuer`, `SigningKey`. |
| `JwtProviderType` | Enum: `Oidc` o `Symmetric`. |

---

## Políticas de Autorización

La política `FallbackPolicy` exige que toda petición esté autenticada. Las políticas
de scope se agregan por recurso:

```csharp
options.AddPolicy("products.read",  policy => policy.RequireClaim("scope", "products.read"));
options.AddPolicy("products.write", policy => policy.RequireClaim("scope", "products.write"));
options.AddPolicy("orders.read",    policy => policy.RequireClaim("scope", "orders.read"));
options.AddPolicy("orders.write",   policy => policy.RequireClaim("scope", "orders.write"));
```

Los Controllers usan `[Authorize(Policy = "products.read")]`.

---

## Agregar un Nuevo Proveedor

1. Agregar entrada en `appsettings.json > Jwt > Providers[]`.
2. Si es `Symmetric`: proveer `Issuer` y `SigningKey` (nunca en texto plano — usar Docker Secrets o env vars).
3. No se requiere código adicional — el `ForwardDefaultSelector` lo detecta automáticamente.

---

## Referencia

- [`docs/PATTERNS.md`](PATTERNS.md) — Patrones CQRS y arquitectura general.
- [`src/Olimpia.Application/Common/Configuration/`](../src/Olimpia.Application/Common/Configuration/) — Clases de configuración.
- [`src/Olimpia.Api/Program.cs`](../src/Olimpia.Api/Program.cs) — Registro multi-proveedor.

---

## 2. Decoradores en Controllers

### [Authorize] - Requiere Token Válido

```csharp
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

[Authorize]
[ApiVersion("1.0")]
public sealed class ProductController : ApiController
{
    [MapToApiVersion("1.0")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Accesible solo con token válido (cualquier scope)
        var product = await _mediator.SendAsync(new GetProductQuery(id));
        return Ok(product);
    }
}
```

### [Authorize(Policy = "...")] - Requiere Scope Específico

```csharp
using Asp.Versioning;

[ApiVersion("1.0")]
public sealed class ProductController : ApiController
{
    [MapToApiVersion("1.0")]
    [HttpPost]
    [Authorize(Policy = "products.write")]  // Requiere scope específico
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var id = await _mediator.SendAsync(command);
        return Created($"api/v1/products/{id}", new { Id = id });
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{id}")]
    [Authorize(Policy = "products.write")]
    public async Task<IActionResult> Update(int id, UpdateProductCommand command)
    {
        var success = await _mediator.SendAsync(command);
        return success ? Ok() : NotFound();
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("{id}")]
    [Authorize(Policy = "products.admin")]  // Requiere admin
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _mediator.SendAsync(new DeleteProductCommand(id));
        return success ? Ok() : NotFound();
    }
}
```

### [AllowAnonymous] - Sin Autenticación

```csharp
[HttpPost("login")]
[AllowAnonymous]
public async Task<IActionResult> Login(LoginRequest request)
{
    // Endpoint sin protección (para testing, etc.)
    return Ok();
}
```

---

## 3. Configurar OpenIddict Authorization Server

En el servidor OpenIddict (fuera de Olimpia), definir el **ApiResource** y **Scopes**:

```csharp
// En OpenIddict Authorization Server config

public static IEnumerable<ApiResource> GetApiResources() =>
    new List<ApiResource>
    {
        new("olimpia-template", "Olimpia API")
        {
            Scopes = 
            {
                "products.read",
                "products.write",
                "products.admin"
            },
            UserClaims = { "sub", "oid", "email", "name" }
        }
    };

public static IEnumerable<ApiScope> GetApiScopes() =>
    new List<ApiScope>
    {
        new("products.read", "Leer productos"),
        new("products.write", "Crear/Editar productos"),
        new("products.admin", "Administración de productos")
    };

// Cliente que obtiene token para acceder a olimpia-template
public static IEnumerable<Client> GetClients() =>
    new List<Client>
    {
        new()
        {
            ClientId = "web-client",
            ClientSecrets = { new Secret("secret123".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = { "http://localhost:3000/callback" },
            AllowedScopes = { "openid", "profile", "email", "products.read", "products.write" }
        }
    };
```

---

## 4. Extraer Claims del Token

### IHttpContextAccessor - Acceso al Contexto HTTP

```csharp
using Microsoft.AspNetCore.Http;

public sealed class ProductController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var context = _httpContextAccessor.HttpContext;
        
        // Obtener usuario desde claims
        var userId = context.User.FindFirst("sub")?.Value
                  ?? context.User.FindFirst("oid")?.Value;
        
        var email = context.User.FindFirst("email")?.Value;
        var scopes = context.User.FindFirst("scope")?.Value?.Split(' ');
        
        // Usar en lógica (e.g., auditoría)
        var enrichedCommand = command with { CreatedBy = userId };
        
        var id = await _mediator.SendAsync(enrichedCommand);
        return Created($"api/products/{id}", new { Id = id });
    }
}
```

### Método Extension para Acceso Limpio

```csharp
public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst("sub")?.Value
        ?? user.FindFirst("oid")?.Value
        ?? user.FindFirst("nameid")?.Value;

    public static string? GetEmail(this ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value;

    public static bool HasScope(this ClaimsPrincipal user, string scope) =>
        user.FindFirst("scope")?.Value?.Split(' ').Contains(scope) ?? false;

    public static IEnumerable<string> GetScopes(this ClaimsPrincipal user) =>
        user.FindFirst("scope")?.Value?.Split(' ') ?? Enumerable.Empty<string>();
}

// Uso
public sealed class ProductController : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var userId = User.GetUserId();
        var hasWriteScope = User.HasScope("products.write");
        
        if (!hasWriteScope)
            return Forbid("No tienes permiso para crear productos");
        
        var id = await _mediator.SendAsync(command);
        return Created($"api/products/{id}", new { Id = id });
    }
}
```

---

## 5. Token Propagation a APIs Externas

Ver [**HTTP_CLIENTS.md**](HTTP_CLIENTS.md) para detalles de `BearerTokenPropagationHandler`.

En resumen: el token del request entrante se reenvía automáticamente a APIs externas.

```csharp
public sealed class GetProductAnalyticsHandler : IQueryHandler<GetProductAnalyticsQuery, AnalyticsDto>
{
    private readonly IExternalApiClient _externalApiClient;

    public async Task<AnalyticsDto> Handle(GetProductAnalyticsQuery query, CancellationToken ct)
    {
        // Token del request entrante se propaga automáticamente
        var analytics = await _externalApiClient.GetAsync<AnalyticsDto>(
            "AnalyticsService", 
            "api/product-analytics", 
            ct);
        
        return analytics;
    }
}
```

---

## 6. Flujo de Validación de Token

```
1. Cliente hace request
   Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjEifQ...

2. ExceptionMiddleware captura la solicitud

3. JwtBearerHandler intercepta
   ├─ Descifra y valida firma con claves públicas de OpenIddict
   ├─ Verifica exp, nbf, iss claims
   ├─ Verifica audience (aud) = "olimpia-template"
   └─ Si todo está bien, crea ClaimsPrincipal

4. User.Identity.IsAuthenticated = true
   User.Claims = { sub, oid, email, scope, ... }

5. [Authorize] pasa ✓
   [Authorize(Policy = "products.write")] valida scope claim

6. Si autorizado, procesa request
   Si no, retorna 403 Forbidden

7. Respuesta
```

---

## 7. Manejo de Excepciones de Autenticación

```csharp
[ApiController]
[Route("api/[controller]")]
public sealed class ProductController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "products.write")]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        try
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized(new { Error = "Token no válido o expirado" });

            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Error = "Usuario no identificado en token" });

            var id = await _mediator.SendAsync(command);
            return Created($"api/products/{id}", new { Id = id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
```

---

## 8. Testing con JWT

### Generar Token de Testing (planificado — aún no implementado)

```csharp
// tests/Olimpia.Tests/Fixtures/JwtTokenGenerator.cs  (pendiente de implementación)
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public static class JwtTokenGenerator
{
    public static string GenerateTestToken(
        string userId = "test-user",
        string email = "test@example.com",
        params string[] scopes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-32-characters-1"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("sub", userId),
            new("email", email),
            new("scope", string.Join(" ", scopes ?? new[] { "products.read" }))
        };

        var token = new JwtSecurityToken(
            issuer: "http://localhost:5001",
            audience: "olimpia-template",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Uso en tests
[TestMethod]
public async Task CreateProduct_WithValidToken_Returns201()
{
    var token = JwtTokenGenerator.GenerateTestToken(scopes: "products.write");
    
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);

    var response = await _client.PostAsJsonAsync("api/products", new CreateProductCommand(...));

    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
}
```

---

## 9. Buenas Prácticas

| Recomendación | Razón |
|---------------|-------|
| ✅ Usar `[Authorize(Policy = "...")]` | Explícito y auditable |
| ❌ Evitar `[Authorize]` sin policy | Poco específico |
| ✅ Validar userId en handler | Asegurar trazabilidad |
| ✅ Loguear intentos de acceso denegado | Seguridad |
| ❌ Almacenar tokens en localStorage (SPA) | CSRF vulnerability |
| ✅ Usar httpOnly cookies para tokens | XSS proof |
| ✅ Renovar tokens regularmente | Expiración |
| ❌ Emitir tokens localmente | Centralizar en OpenIddict |

---

## Próximos Pasos

- **[HTTP_CLIENTS.md](HTTP_CLIENTS.md)** - Token propagation en llamadas externas
- **[CONFIGURATION.md](CONFIGURATION.md)** - Variables de entorno JWT
