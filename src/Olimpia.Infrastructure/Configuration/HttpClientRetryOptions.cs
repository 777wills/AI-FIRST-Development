// Inicio código generado por GitHub Copilot
namespace Olimpia.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para reintentos en clientes HTTP.
/// Se enlaza con la sección "HttpClient" de appsettings.json.
/// </summary>
public sealed class HttpClientRetryOptions
{
    /// <summary>
    /// Indica si los reintentos están habilitados.
    /// </summary>
    public bool RetryEnabled { get; set; } = true;

    /// <summary>
    /// Número máximo de intentos de reintento.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Demora inicial en milisegundos antes del primer reintento.
    /// Se aplica backoff exponencial en reintentos subsecuentes.
    /// </summary>
    public int InitialDelayMs { get; set; } = 200;
}
// Fin código generado por GitHub Copilot
