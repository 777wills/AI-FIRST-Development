// Inicio código generado por GitHub Copilot
namespace Olimpia.Application.Common.Configuration;

/// <summary>
/// Configuración de un proveedor JWT individual.
/// Soporta tanto proveedores OIDC (OpenIddict/Keycloak) como proveedores simétricos (HS256).
/// </summary>
public sealed class JwtProviderOptions
{
    /// <summary>Nombre único del esquema de autenticación (ej. "OpenIddict", "Symmetric").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Tipo de proveedor: OIDC o Simétrico.</summary>
    public JwtProviderType Type { get; set; } = JwtProviderType.Oidc;

    /// <summary>Indica si este proveedor está activo.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>URL de autoridad para proveedores OIDC (endpoint de discovery).</summary>
    public string? Authority { get; set; }

    /// <summary>Audience esperada en el token JWT.</summary>
    public string? Audience { get; set; }

    /// <summary>Indica si se requiere HTTPS para el metadata del proveedor OIDC.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>Issuer esperado (obligatorio para proveedores simétricos).</summary>
    public string? Issuer { get; set; }

    /// <summary>Clave de firma para proveedores simétricos (HS256). Leer desde Docker Secrets / env vars.</summary>
    public string? SigningKey { get; set; }
}
// Fin código generado por GitHub Copilot
