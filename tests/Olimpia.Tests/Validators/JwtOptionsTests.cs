// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Olimpia.Application.Common.Configuration;

namespace Olimpia.Tests.Validators;

/// <summary>
/// Pruebas unitarias para las clases de configuración JWT multi-proveedor.
/// </summary>
[TestClass]
public sealed class JwtOptionsTests
{
    // Método generado por GitHub Copilot
    [TestMethod]
    public void JwtOptions_DefaultProviders_Should_BeEmptyList()
    {
        // Arrange & Act
        var options = new JwtOptions();

        // Assert
        options.Providers.Should().BeEmpty();
    }

    [TestMethod]
    public void JwtProviderOptions_DefaultType_Should_BeOidc()
    {
        // Arrange & Act
        var provider = new JwtProviderOptions();

        // Assert
        provider.Type.Should().Be(JwtProviderType.Oidc);
    }

    [TestMethod]
    public void JwtProviderOptions_DefaultEnabled_Should_BeTrue()
    {
        // Arrange & Act
        var provider = new JwtProviderOptions();

        // Assert
        provider.Enabled.Should().BeTrue();
    }

    [TestMethod]
    public void JwtProviderOptions_DefaultRequireHttpsMetadata_Should_BeTrue()
    {
        // Arrange & Act
        var provider = new JwtProviderOptions();

        // Assert
        provider.RequireHttpsMetadata.Should().BeTrue();
    }

    [TestMethod]
    public void JwtOptions_AddOidcProvider_Should_BePresentInList()
    {
        // Arrange
        var oidcProvider = new JwtProviderOptions
        {
            Name = "OpenIddict",
            Type = JwtProviderType.Oidc,
            Enabled = true,
            Authority = "https://auth.example.com",
            Audience = "olimpia-api"
        };

        // Act
        var options = new JwtOptions();
        options.Providers.Add(oidcProvider);

        // Assert
        options.Providers.Should().ContainSingle(p =>
            p.Name == "OpenIddict" &&
            p.Type == JwtProviderType.Oidc &&
            p.Enabled);
    }

    [TestMethod]
    public void JwtOptions_AddSymmetricProvider_Should_BePresentInList()
    {
        // Arrange
        var symmetricProvider = new JwtProviderOptions
        {
            Name = "Internal",
            Type = JwtProviderType.Symmetric,
            Enabled = true,
            Issuer = "olimpia-internal",
            SigningKey = "supersecretkey1234567890abcdefgh"
        };

        // Act
        var options = new JwtOptions();
        options.Providers.Add(symmetricProvider);

        // Assert
        options.Providers.Should().ContainSingle(p =>
            p.Name == "Internal" &&
            p.Type == JwtProviderType.Symmetric &&
            p.Issuer == "olimpia-internal");
    }

    [TestMethod]
    public void JwtOptions_FilterEnabledProviders_Should_ReturnOnlyEnabled()
    {
        // Arrange
        var options = new JwtOptions
        {
            Providers =
            [
                new JwtProviderOptions { Name = "Active", Enabled = true },
                new JwtProviderOptions { Name = "Inactive", Enabled = false }
            ]
        };

        // Act
        var enabled = options.Providers.Where(p => p.Enabled).ToList();

        // Assert
        enabled.Should().ContainSingle(p => p.Name == "Active");
    }
}
// Fin código generado por GitHub Copilot
