using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Utils;
using Xunit;

public class AzureKeyVaultServicesTests
{
    private readonly Mock<SecretClient> _mockSecretClient;
    private readonly IOptions<KeyVaultSettings> _options;
    private readonly KeyVaultSettings _settings;

    public AzureKeyVaultServicesTests()
    {
        _mockSecretClient = new Mock<SecretClient>();
        _settings = new KeyVaultSettings
        {
            VaultUrl = "https://myvault.vault.azure.net/",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            TenantId = "tenant-id"
        };
        _options = Options.Create(_settings);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenVaultUrlIsNullOrEmpty()
    {
        // Arrange
        var options = Options.Create(new KeyVaultSettings { VaultUrl = null });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AzureKeyVaultServices(options));
    }

    [Fact]
    public void Constructor_ShouldInitializeSecretClient_WithClientSecretCredential()
    {
        // Act
        var service = new AzureKeyVaultServices(_options);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SetSecretAsync_ShouldCallSetSecretAsync_OnSecretClient()
    {
        // Arrange
        var service = new AzureKeyVaultServices(_options);
        var key = "test-key";
        var value = "test-value";

        _mockSecretClient.Setup(client => client.SetSecretAsync(key, value, default))
            .ReturnsAsync(Response.FromValue(new KeyVaultSecret(key, value), null));

        // Act
        await service.SetSecretAsync(key, value);

        // Assert
        _mockSecretClient.Verify(client => client.SetSecretAsync(key, value, default), Times.Once);
    }

    [Fact]
    public async Task GetSecretAsync_ShouldReturnSecretValue()
    {
        // Arrange
        var service = new AzureKeyVaultServices(_options);
        var key = "test-key";
        var expectedValue = "test-value";

        _mockSecretClient.Setup(client => client.GetSecretAsync(key, null, default))
            .ReturnsAsync(Response.FromValue(new KeyVaultSecret(key, expectedValue), null));

        // Act
        var result = await service.GetSecretAsync(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task DeleteSecretAsync_ShouldCallStartDeleteSecretAsync_OnSecretClient()
    {
        // Arrange
        var service = new AzureKeyVaultServices(_options);
        var key = "test-key";
        var secret = new DeleteSecretOperation()

        _mockSecretClient.Setup(client => client.StartDeleteSecretAsync(key, default))
            .ReturnsAsync(Response.FromValue(new DeleteSecretOperation(new Mock<Response>().Object), null));

        // Act
        await service.DeleteSecretAsync(key);

        // Assert
        _mockSecretClient.Verify(client => client.StartDeleteSecretAsync(key, default), Times.Once);
    }
}
