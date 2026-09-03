// Inicio código generado por GitHub Copilot
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi;

namespace Olimpia.Api.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Registra Swagger con documentación multi-versión y seguridad Bearer.
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            // Inicio refactorización/optimización por GitHub Copilot
            // CustomSchemaIds evita colisiones de nombre cuando existen records anidados o
            // tipos con el mismo nombre simple en distintos namespaces (ej. Command vs Response).
            options.CustomSchemaIds(type => type.FullName!.Replace('+', '.'));
            options.OperationFilter<PaginatedEndpointOperationFilter>();
            // Fin refactorización/optimización por GitHub Copilot

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "Bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = $"Token emitido por OpenIddict ({configuration["Jwt:Providers:0:Authority"] ?? "servidor de autenticación"}).\nEjemplo: Bearer eyJhbGci..."
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc),
                    []
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configura Swagger UI con endpoints por cada versión de la API.
    /// </summary>
    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();
            foreach (var description in descriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"Olimpia API {description.GroupName.ToUpperInvariant()}");
            }
        });

        return app;
    }
}
// Fin código generado por GitHub Copilot
