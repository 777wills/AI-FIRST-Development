// Inicio código generado por GitHub Copilot
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Olimpia.Api.Extensions;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title       = $"Olimpia API {description.GroupName.ToUpperInvariant()}",
                Version     = description.GroupName,
                Description = description.IsDeprecated
                    ? "Esta versión de la API está obsoleta."
                    : "API REST de Olimpia."
            });
        }
    }
}
// Fin código generado por GitHub Copilot
