// Inicio código generado por GitHub Copilot
using Microsoft.AspNetCore.Http;

namespace Olimpia.Infrastructure.Http;

/// <summary>
/// DelegatingHandler que lee el Bearer token del request HTTP entrante
/// (a través de IHttpContextAccessor) y lo añade automáticamente como
/// header "Authorization: Bearer ..." en cada llamada saliente a APIs externas.
///
/// Si no hay request activo (ej. jobs en background), la llamada saliente
/// se realiza sin token.
/// </summary>
public sealed class BearerTokenPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Método generado por GitHub Copilot
    public BearerTokenPropagationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Método generado por GitHub Copilot
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        var token = ExtractBearerToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        // Fin refactorización/optimización por GitHub Copilot

        return await base.SendAsync(request, cancellationToken);
    }

    // Método generado por GitHub Copilot
    private string ExtractBearerToken()
    {
        var authHeader = _httpContextAccessor.HttpContext?
            .Request.Headers.Authorization
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return authHeader["Bearer ".Length..].Trim();
    }
}
// Fin código generado por GitHub Copilot
