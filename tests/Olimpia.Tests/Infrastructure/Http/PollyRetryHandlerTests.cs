// Inicio código generado por GitHub Copilot
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Olimpia.Infrastructure.Configuration;
using Olimpia.Infrastructure.Http;
using System.Net;

namespace Olimpia.Tests.Infrastructure.Http;

[TestClass]
public sealed class PollyRetryHandlerTests
{
    private Mock<ILogger<PollyRetryHandler>> _mockLogger = null!;
    private Mock<HttpMessageHandler> _mockInnerHandler = null!;
    private PollyRetryHandler _pollyRetryHandler = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLogger = new Mock<ILogger<PollyRetryHandler>>();
        _mockInnerHandler = new Mock<HttpMessageHandler>();

        // Inicio código generado por GitHub Copilot
        // Crear opciones de configuración para testing
        var retryOptions = Options.Create(new HttpClientRetryOptions
        {
            RetryEnabled = true,
            MaxRetryAttempts = 3,
            InitialDelayMs = 200
        });
        // Fin código generado por GitHub Copilot

        _pollyRetryHandler = new PollyRetryHandler(_mockLogger.Object, retryOptions)
        {
            InnerHandler = _mockInnerHandler.Object
        };

        _httpClient = new HttpClient(_pollyRetryHandler)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _httpClient?.Dispose();
        _pollyRetryHandler?.Dispose();
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task WhenServiceUnavailable_ThenRetriesAutomatically()
    {
        // Arrange
        var attempts = 0;

        _mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attempts++;
                if (attempts < 3)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent("Service Unavailable")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Success")
                };
            });

        // Act
        var response = await _httpClient.GetAsync("/api/test");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(3, attempts); // Se reintentó 2 veces antes de tener éxito

        _mockInnerHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task WhenHttpRequestException_ThenRetriesAutomatically()
    {
        // Arrange
        var attempts = 0;

        _mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attempts++;
                if (attempts < 3)
                {
                    throw new HttpRequestException("Connection error");
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Success")
                };
            });

        // Act
        var response = await _httpClient.GetAsync("/api/test");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(3, attempts);
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task WhenBadRequest_ThenDoesNotRetry()
    {
        // Arrange
        var attempts = 0;

        _mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attempts++;
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad Request")
                };
            });

        // Act
        var response = await _httpClient.GetAsync("/api/test");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual(1, attempts); // No reintenta errores 4xx

        _mockInnerHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task WhenGatewayTimeout_ThenRetriesAutomatically()
    {
        // Arrange
        var attempts = 0;

        _mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attempts++;
                // Siempre retorna GatewayTimeout para simular que todos los reintentos se agotan.
                return new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
                {
                    Content = new StringContent("Gateway Timeout")
                };
            });

        // Act
        var response = await _httpClient.GetAsync("/api/test");

        // Assert
        Assert.IsNotNull(response);
        // Después de 3 reintentos (4 intentos totales), sigue siendo GatewayTimeout
        Assert.AreEqual(HttpStatusCode.GatewayTimeout, response.StatusCode);
        Assert.AreEqual(4, attempts); // Intento inicial + 3 reintentos
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task WhenTooManyRequests_ThenRetriesAutomatically()
    {
        // Arrange
        var attempts = 0;

        _mockInnerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                attempts++;
                if (attempts < 2)
                {
                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                    {
                        Content = new StringContent("Rate limit exceeded")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Success")
                };
            });

        // Act
        var response = await _httpClient.GetAsync("/api/test");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(2, attempts); // Reintentó 1 vez después de 429
    }
}
// Fin código generado por GitHub Copilot
