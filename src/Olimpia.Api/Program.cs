using Olimpia.Application;
using Olimpia.Application.Common.Configuration;
using Olimpia.Infrastructure;
using OlimpiaIT.Logging.Serilog.Database;
using Olimpia.Api.Middleware;
using Olimpia.Api.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Inicio código generado por GitHub Copilot
// Las variables de entorno sobreescriben appsettings.json.
// Usar __ (doble guion bajo) como separador de secciones jerárquicas.
// Ejemplo: ConnectionStrings__DefaultConnection, Logging__CustomLogger__MinimumLevel
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddDockerSecrets(); // Cargar secretos desde /run/secrets/ (Docker/Kubernetes/Podman)
// Fin código generado por GitHub Copilot

// Inicio código generado por GitHub Copilot
// Registrar IDbProvider y opciones del sink de BD de logs (OlimpiaIT.Logging.Serilog.Database)
builder.Services.AddLogCentralDatabase(builder.Configuration);

// Configurar Serilog como motor de logging con Console + File + DB (sin HTTP a LogCentral)
builder.Host.UseSerilogWithDatabaseOnly(builder.Configuration);
// Fin código generado por GitHub Copilot

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration(builder.Configuration);

// Inicio código generado por GitHub Copilot
// Autenticación multi-proveedor JWT via PolicyScheme + ForwardDefaultSelector.
// Permite convivir: OIDC (OpenIddict/Keycloak) + clave simétrica (HS256).
// El selector inspecciona el claim "iss" del token para determinar el esquema.
// Variables de entorno: Jwt__Providers__0__Authority, Jwt__Providers__1__SigningKey, etc.
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (!jwtOptions.Providers.Any(p => p.Enabled))
{
    throw new InvalidOperationException("No hay proveedores JWT habilitados en la configuración. Revise la sección 'Jwt:Providers' en appsettings.json.");
}

var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "JwtMultiProvider";
        options.DefaultChallengeScheme = "JwtMultiProvider";
    })
    .AddPolicyScheme("JwtMultiProvider", "JWT Multi-Provider", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            string? authHeader = context.Request.Headers.Authorization;
            if (!string.IsNullOrEmpty(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                string rawToken = authHeader["Bearer ".Length..].Trim();
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    if (handler.CanReadToken(rawToken))
                    {
                        string? iss = handler.ReadJwtToken(rawToken).Issuer;

                        // Si el issuer coincide con un proveedor simétrico habilitado, usarlo.
                        var symmetricMatch = jwtOptions.Providers.FirstOrDefault(p =>
                            p.Enabled &&
                            p.Type == JwtProviderType.Symmetric &&
                            string.Equals(p.Issuer, iss, StringComparison.OrdinalIgnoreCase));

                        if (symmetricMatch is not null)
                            return symmetricMatch.Name;
                    }
                }
                catch { /* Token mal formado — caer al proveedor OIDC por defecto. */ }
            }

            // Por defecto, usar el primer proveedor OIDC habilitado.
            return jwtOptions.Providers
                .FirstOrDefault(p => p.Enabled && p.Type == JwtProviderType.Oidc)
                ?.Name ?? JwtBearerDefaults.AuthenticationScheme;
        };
    });

foreach (var provider in jwtOptions.Providers.Where(p => p.Enabled))
{
    if (provider.Type == JwtProviderType.Oidc)
    {
        authBuilder.AddJwtBearer(provider.Name, options =>
        {
            options.Authority = provider.Authority;
            options.Audience = provider.Audience;
            options.RequireHttpsMetadata = provider.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role",
                ValidateAudience = provider.Audience is not null,
                ValidAudiences = provider.Audience is not null ? [provider.Audience] : null,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateTokenReplay = false,
            };
            options.BackchannelTimeout = TimeSpan.FromSeconds(10);
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearer");
                    logger.LogWarning("[JwtBearer:{Scheme}] OnAuthenticationFailed: {Error}", provider.Name, ctx.Exception.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearer");
                    logger.LogWarning(
                        "[JwtBearer:{Scheme}] OnChallenge — Error: {Error} | ErrorDescription: {Desc}",
                        provider.Name, ctx.Error, ctx.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });
    }
    else if (provider.Type == JwtProviderType.Symmetric)
    {
        var signingKeyBytes = Encoding.UTF8.GetBytes(provider.SigningKey ?? string.Empty);
        authBuilder.AddJwtBearer(provider.Name, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
                ValidateIssuer = true,
                ValidIssuer = provider.Issuer,
                ValidateAudience = provider.Audience is not null,
                ValidAudiences = provider.Audience is not null ? [provider.Audience] : null,
                ValidateLifetime = false,
                NameClaimType = "name",
                RoleClaimType = "role",
            };
        });
    }
}

// Política de autorización con fallback: toda petición autenticada requiere al menos un claim de identidad.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder("JwtMultiProvider")
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("products.read",  policy => policy.RequireClaim("scope", "products.read"));
    options.AddPolicy("products.write", policy => policy.RequireClaim("scope", "products.write"));
    options.AddPolicy("orders.read",    policy => policy.RequireClaim("scope", "orders.read"));
    options.AddPolicy("orders.write",   policy => policy.RequireClaim("scope", "orders.write"));
});
// Fin código generado por GitHub Copilot

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwaggerConfiguration();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<AuditMiddleware>();

app.UseHttpsRedirection();

// Inicio código generado por GitHub Copilot
app.UseAuthentication();
app.UseAuthorization();
// Fin código generado por GitHub Copilot

// Inicio código generado por GitHub Copilot
// Health Check endpoint usando Minimal API
// Retorna status "Ok" y timestamp en hora local del servidor.
app.MapGet("/api/health", () =>
{
    var response = new
    {
        status = "Ok",
        timestamp = DateTime.Now
    };
    return Results.Ok(response);
})
.WithName("HealthCheck")
.WithTags("Health")
.WithOpenApi(operation =>
{
    operation.Summary = "Health Check";
    operation.Description = "Verifica el estado del servicio. Retorna status 'Ok' y timestamp actual.";
    return operation;
})
.AllowAnonymous();
// Fin código generado por GitHub Copilot

app.MapControllers();

await app.RunAsync();

