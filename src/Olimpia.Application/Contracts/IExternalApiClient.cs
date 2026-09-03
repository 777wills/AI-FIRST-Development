// Inicio código generado por GitHub Copilot
namespace Olimpia.Application.Contracts;

/// <summary>
/// Contrato para consumir APIs externas desde los casos de uso (Handlers).
/// La implementación en Infrastructure propaga automáticamente el Bearer token
/// del request entrante hacia las llamadas salientes.
/// </summary>
public interface IExternalApiClient
{
    /// <summary>
    /// Realiza un GET y deserializa la respuesta a <typeparamref name="TResponse"/>.
    /// </summary>
    // Método generado por GitHub Copilot
    Task<TResponse?> GetAsync<TResponse>(string clientName, string relativeUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza un POST enviando <paramref name="payload"/> como JSON.
    /// </summary>
    // Método generado por GitHub Copilot
    Task<TResponse?> PostAsync<TRequest, TResponse>(string clientName, string relativeUri, TRequest payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza un PUT enviando <paramref name="payload"/> como JSON.
    /// </summary>
    // Método generado por GitHub Copilot
    Task<TResponse?> PutAsync<TRequest, TResponse>(string clientName, string relativeUri, TRequest payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza un DELETE.
    /// </summary>
    // Método generado por GitHub Copilot
    Task DeleteAsync(string clientName, string relativeUri, CancellationToken cancellationToken = default);
}
// Fin código generado por GitHub Copilot
