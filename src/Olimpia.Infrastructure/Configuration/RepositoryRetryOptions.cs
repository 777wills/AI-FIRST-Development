// Inicio código generado por GitHub Copilot
namespace Olimpia.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para reintentos en repositorios de base de datos.
/// Se enlaza con la sección "Repository" de appsettings.json.
/// </summary>
public sealed class RepositoryRetryOptions
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
    public int InitialDelayMs { get; set; } = 100;
}
// Fin código generado por GitHub Copilot
