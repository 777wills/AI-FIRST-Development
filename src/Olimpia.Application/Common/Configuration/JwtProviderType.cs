// Inicio código generado por GitHub Copilot
namespace Olimpia.Application.Common.Configuration;

/// <summary>
/// Tipo de proveedor JWT: OIDC (OpenIddict / Keycloak) o Simétrico (clave compartida).
/// </summary>
public enum JwtProviderType
{
    /// <summary>Proveedor OIDC — descarga claves públicas desde el endpoint de discovery.</summary>
    Oidc,

    /// <summary>Proveedor simétrico — valida con una clave compartida (HS256).</summary>
    Symmetric
}
// Fin código generado por GitHub Copilot
