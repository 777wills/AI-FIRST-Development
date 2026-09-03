// Inicio código generado por GitHub Copilot
namespace Olimpia.Application.Common.Configuration;

/// <summary>
/// Opciones raíz de autenticación JWT multi-proveedor.
/// Se enlaza con la sección "Jwt" de appsettings.json.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>Lista de proveedores JWT habilitados en la aplicación.</summary>
    public List<JwtProviderOptions> Providers { get; set; } = [];
}
// Fin código generado por GitHub Copilot
