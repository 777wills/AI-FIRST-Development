// Inicio código generado por GitHub Copilot
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olimpia.Infrastructure.Configuration;

namespace Olimpia.Tests.Infrastructure.Configuration;

[TestClass]
public sealed class RetryOptionsConfigurationTests
{
    // Método generado por GitHub Copilot
    [TestMethod]
    public void WhenHttpClientRetryOptionsConfigured_ThenValuesAreReadCorrectly()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            ["HttpClient:RetryEnabled"] = "true",
            ["HttpClient:MaxRetryAttempts"] = "5",
            ["HttpClient:InitialDelayMs"] = "300"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<HttpClientRetryOptions>(configuration.GetSection("HttpClient"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<HttpClientRetryOptions>>().Value;

        // Assert
        Assert.IsTrue(options.RetryEnabled);
        Assert.AreEqual(5, options.MaxRetryAttempts);
        Assert.AreEqual(300, options.InitialDelayMs);
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public void WhenRepositoryRetryOptionsConfigured_ThenValuesAreReadCorrectly()
    {
        // Arrange
        var configurationData = new Dictionary<string, string?>
        {
            ["Repository:RetryEnabled"] = "false",
            ["Repository:MaxRetryAttempts"] = "2",
            ["Repository:InitialDelayMs"] = "50"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<RepositoryRetryOptions>(configuration.GetSection("Repository"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<RepositoryRetryOptions>>().Value;

        // Assert
        Assert.IsFalse(options.RetryEnabled);
        Assert.AreEqual(2, options.MaxRetryAttempts);
        Assert.AreEqual(50, options.InitialDelayMs);
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public void WhenNoConfigurationProvided_ThenDefaultValuesAreUsed()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.Configure<HttpClientRetryOptions>(configuration.GetSection("HttpClient"));
        services.Configure<RepositoryRetryOptions>(configuration.GetSection("Repository"));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var httpOptions = serviceProvider.GetRequiredService<IOptions<HttpClientRetryOptions>>().Value;
        var repoOptions = serviceProvider.GetRequiredService<IOptions<RepositoryRetryOptions>>().Value;

        // Assert
        // Valores por defecto de HttpClientRetryOptions
        Assert.IsTrue(httpOptions.RetryEnabled);
        Assert.AreEqual(3, httpOptions.MaxRetryAttempts);
        Assert.AreEqual(200, httpOptions.InitialDelayMs);

        // Valores por defecto de RepositoryRetryOptions
        Assert.IsTrue(repoOptions.RetryEnabled);
        Assert.AreEqual(3, repoOptions.MaxRetryAttempts);
        Assert.AreEqual(100, repoOptions.InitialDelayMs);
    }
}
// Fin código generado por GitHub Copilot
