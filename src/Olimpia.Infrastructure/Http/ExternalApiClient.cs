// Inicio código generado por GitHub Copilot
using System.Net.Http.Json;
using Olimpia.Application.Contracts;

namespace Olimpia.Infrastructure.Http;

/// <summary>
/// Implementación de <see cref="IExternalApiClient"/> que usa <see cref="IHttpClientFactory"/>
/// para crear clientes con nombre. Cada cliente debe estar registrado con
/// <c>services.AddHttpClient("nombre", c => c.BaseAddress = new Uri(...))</c>.
///
/// El Bearer token del request entrante se propaga automáticamente via
/// <see cref="BearerTokenPropagationHandler"/>.
/// </summary>
internal sealed class ExternalApiClient : IExternalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    // Método generado por GitHub Copilot
    public ExternalApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Método generado por GitHub Copilot
    public async Task<TResponse?> GetAsync<TResponse>(
        string clientName,
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(clientName);
        var response = await client.GetAsync(relativeUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    // Método generado por GitHub Copilot
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string clientName,
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(clientName);
        var response = await client.PostAsJsonAsync(relativeUri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    // Método generado por GitHub Copilot
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string clientName,
        string relativeUri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(clientName);
        var response = await client.PutAsJsonAsync(relativeUri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    // Método generado por GitHub Copilot
    public async Task DeleteAsync(
        string clientName,
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(clientName);
        var response = await client.DeleteAsync(relativeUri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
// Fin código generado por GitHub Copilot
