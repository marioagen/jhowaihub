using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Infrastructure.Multitenancy;
using WoopiAiHub.Repository.Context;
using Xunit;

namespace WoopiAiHub.UnitTests.Infrastructure.Multitenancy
{
    public class TenantContextServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly TenantContextService _sut;
        private const string ValidConnectionString = "Server=.;Database=___NEWDB___;User Id=sa;Password=123456;";
        private const string TemplateConnectionKey = "TemplateConnection";

        public TenantContextServiceTests()
        {
            _mocker = new AutoMocker();

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { $"ConnectionStrings:{TemplateConnectionKey}", ValidConnectionString }
                })
                .Build();

            _mocker.Use<IConfiguration>(configBuilder);
            _sut = _mocker.CreateInstance<TenantContextService>();
        }

        #region InitializeTenantAsync Tests

        [Fact(DisplayName = "InitializeTenantAsync - Should throw ArgumentException when tenant identifier is null")]
        [Trait("InitializeTenantAsync", "Validation")]
        public async Task InitializeTenantAsync_WithNullTenantIdentifier_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.InitializeTenantAsync(null!));

            Assert.Equal("Tenant identifier cannot be null or empty. (Parameter 'tenantIdentifier')", exception.Message);
        }

        [Fact(DisplayName = "InitializeTenantAsync - Should throw ArgumentException when tenant identifier is empty")]
        [Trait("InitializeTenantAsync", "Validation")]
        public async Task InitializeTenantAsync_WithEmptyTenantIdentifier_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.InitializeTenantAsync(string.Empty));

            Assert.Equal("Tenant identifier cannot be null or empty. (Parameter 'tenantIdentifier')", exception.Message);
        }

        [Fact(DisplayName = "InitializeTenantAsync - Should throw ArgumentException when tenant identifier is whitespace")]
        [Trait("InitializeTenantAsync", "Validation")]
        public async Task InitializeTenantAsync_WithWhitespaceTenantIdentifier_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.InitializeTenantAsync("   "));

            Assert.Equal("Tenant identifier cannot be null or empty. (Parameter 'tenantIdentifier')", exception.Message);
        }

        [Fact(DisplayName = "InitializeTenantAsync - Should throw InvalidOperationException when tenant not found")]
        [Trait("InitializeTenantAsync", "NotFound")]
        public async Task InitializeTenantAsync_WhenTenantNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var tenantIdentifier = "non-existent-tenant";
            var tenantCacheServiceMock = _mocker.GetMock<ITenantCacheServices>();

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantIdentifier))
                .ReturnsAsync((TenantInfoDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.InitializeTenantAsync(tenantIdentifier));

            Assert.Equal($"Tenant '{tenantIdentifier}' not found.", exception.Message);
        }

        [Fact(DisplayName = "InitializeTenantAsync - Should successfully initialize tenant database")]
        [Trait("InitializeTenantAsync", "Success")]
        public async Task InitializeTenantAsync_WithValidTenant_SuccessfullyInitializes()
        {
            // Arrange
            var tenantIdentifier = "tenant-123";
            var tenant = new TenantInfoDto { Name = tenantIdentifier, DatabaseName = "TenantDb" };
            var tenantCacheServiceMock = _mocker.GetMock<ITenantCacheServices>();

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantIdentifier))
                .ReturnsAsync(tenant);

            // Act
            await _sut.InitializeTenantAsync(tenantIdentifier);

            // Assert
            tenantCacheServiceMock.Verify(
                x => x.FindTenantAsync(tenantIdentifier),
                Times.Once);
        }

        #endregion

        #region TrySetTenantConnectionAsync Tests

        [Fact(DisplayName = "TrySetTenantConnectionAsync - Should return false when tenant identifier is null")]
        [Trait("TrySetTenantConnectionAsync", "Validation")]
        public async Task TrySetTenantConnectionAsync_WithNullTenantIdentifier_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = await _sut.TrySetTenantConnectionAsync(context, null!);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "TrySetTenantConnectionAsync - Should return false when tenant identifier is empty")]
        [Trait("TrySetTenantConnectionAsync", "Validation")]
        public async Task TrySetTenantConnectionAsync_WithEmptyTenantIdentifier_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = await _sut.TrySetTenantConnectionAsync(context, string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "TrySetTenantConnectionAsync - Should return false when tenant identifier is whitespace")]
        [Trait("TrySetTenantConnectionAsync", "Validation")]
        public async Task TrySetTenantConnectionAsync_WithWhitespaceTenantIdentifier_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = await _sut.TrySetTenantConnectionAsync(context, "   ");

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "TrySetTenantConnectionAsync - Should return false when tenant not found")]
        [Trait("TrySetTenantConnectionAsync", "NotFound")]
        public async Task TrySetTenantConnectionAsync_WhenTenantNotFound_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var tenantIdentifier = "non-existent-tenant";
            var tenantCacheServiceMock = _mocker.GetMock<ITenantCacheServices>();

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantIdentifier))
                .ReturnsAsync((TenantInfoDto?)null);

            // Act
            var result = await _sut.TrySetTenantConnectionAsync(context, tenantIdentifier);

            // Assert
            Assert.False(result);
            Assert.False(context.Items.ContainsKey("TenantConnection"));
        }

        [Fact(DisplayName = "TrySetTenantConnectionAsync - Should successfully set tenant connection in HttpContext")]
        [Trait("TrySetTenantConnectionAsync", "Success")]
        public async Task TrySetTenantConnectionAsync_WithValidTenant_SuccessfullySetsConnection()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var tenantIdentifier = "tenant-123";
            var tenant = new TenantInfoDto { Name = tenantIdentifier, DatabaseName = "TenantDb" };
            var expectedConnectionString = "Server=.;Database=TenantDb;User Id=sa;Password=123456;";
            var tenantCacheServiceMock = _mocker.GetMock<ITenantCacheServices>();

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantIdentifier))
                .ReturnsAsync(tenant);

            // Act
            var result = await _sut.TrySetTenantConnectionAsync(context, tenantIdentifier);

            // Assert
            Assert.True(result);
            Assert.True(context.Items.ContainsKey("TenantConnection"));
            Assert.Equal(expectedConnectionString, context.Items["TenantConnection"]);

            tenantCacheServiceMock.Verify(
                x => x.FindTenantAsync(tenantIdentifier),
                Times.Once);
        }

        #endregion

        #region GetConnectionStringAndHttpAcessorAsync Tests

        [Fact(DisplayName = "GetConnectionStringAndHttpAcessorAsync - Should return connection string and http accessor for valid tenant")]
        [Trait("GetConnectionStringAndHttpAcessorAsync", "Success")]
        public async Task GetConnectionStringAndHttpAcessorAsync_WithValidTenant_ReturnsConnectionStringAndAccessor()
        {
            // Arrange
            var tenantName = "tenant-123";
            var tenant = new TenantInfoDto { Name = tenantName, DatabaseName = "TenantDb" };
            var expectedConnectionString = "Server=.;Database=TenantDb;User Id=sa;Password=123456;";

            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var tenantCacheServiceMock = new Mock<ITenantCacheServices>();
            var httpAccessor = new Mock<IHttpContextAccessor>();

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(ITenantCacheServices)))
                .Returns(tenantCacheServiceMock.Object);

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(IHttpContextAccessor)))
                .Returns(httpAccessor.Object);

            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            var scopeFactoryMock = _mocker.GetMock<IServiceScopeFactory>();
            scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantName))
                .ReturnsAsync(tenant);

            // Act
            var (connectionString, returnedAccessor) = await _sut.FindConnectionStringAndHttpAcessorAsync(tenantName);

            // Assert
            Assert.Equal(expectedConnectionString, connectionString);
            Assert.Equal(httpAccessor.Object, returnedAccessor);

            scopeFactoryMock.Verify(
                x => x.CreateScope(),
                Times.Once);

            tenantCacheServiceMock.Verify(
                x => x.FindTenantAsync(tenantName),
                Times.Once);
        }

        [Fact(DisplayName = "GetConnectionStringAndHttpAcessorAsync - Should return empty string when template connection is not configured")]
        [Trait("GetConnectionStringAndHttpAcessorAsync", "Configuration")]
        public async Task GetConnectionStringAndHttpAcessorAsync_WhenTemplateConnectionNotConfigured_ReturnsEmptyString()
        {
            // Arrange
            var tenantName = "tenant-123";
            var tenant = new TenantInfoDto { Name = tenantName, DatabaseName = "TenantDb" };

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            var mocker = new AutoMocker();
            mocker.Use<IConfiguration>(configBuilder);
            var sut = mocker.CreateInstance<TenantContextService>();

            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var tenantCacheServiceMock = new Mock<ITenantCacheServices>();
            var httpAccessor = new Mock<IHttpContextAccessor>();

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(ITenantCacheServices)))
                .Returns(tenantCacheServiceMock.Object);

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(IHttpContextAccessor)))
                .Returns(httpAccessor.Object);

            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            var scopeFactoryMock = mocker.GetMock<IServiceScopeFactory>();
            scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantName))
                .ReturnsAsync(tenant);

            // Act
            var (connectionString, returnedAccessor) = await sut.FindConnectionStringAndHttpAcessorAsync(tenantName);

            // Assert
            Assert.Equal(string.Empty, connectionString);
            Assert.Equal(httpAccessor.Object, returnedAccessor);
        }

        [Fact(DisplayName = "GetConnectionStringAndHttpAcessorAsync - Should properly format connection string with tenant database name")]
        [Trait("GetConnectionStringAndHttpAcessorAsync", "Success")]
        public async Task GetConnectionStringAndHttpAcessorAsync_ShouldFormatConnectionStringWithTenantDbName()
        {
            // Arrange
            var tenantName = "client-abc";
            var databaseName = "WoopiDb_ClientABC";
            var tenant = new TenantInfoDto { Name = tenantName, DatabaseName = databaseName };
            var expectedConnectionString = "Server=.;Database=WoopiDb_ClientABC;User Id=sa;Password=123456;";

            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var tenantCacheServiceMock = new Mock<ITenantCacheServices>();
            var httpAccessor = new Mock<IHttpContextAccessor>();

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(ITenantCacheServices)))
                .Returns(tenantCacheServiceMock.Object);

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(IHttpContextAccessor)))
                .Returns(httpAccessor.Object);

            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            var scopeFactoryMock = _mocker.GetMock<IServiceScopeFactory>();
            scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantName))
                .ReturnsAsync(tenant);

            // Act
            var (connectionString, _) = await _sut.FindConnectionStringAndHttpAcessorAsync(tenantName);

            // Assert
            Assert.Contains(databaseName, connectionString);
            Assert.Equal(expectedConnectionString, connectionString);
        }

        [Fact(DisplayName = "GetConnectionStringAndHttpAcessorAsync - Should dispose scope after use")]
        [Trait("GetConnectionStringAndHttpAcessorAsync", "ResourceManagement")]
        public async Task GetConnectionStringAndHttpAcessorAsync_ShouldDisposeScope()
        {
            // Arrange
            var tenantName = "tenant-123";
            var tenant = new TenantInfoDto { Name = tenantName, DatabaseName = "TenantDb" };

            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var tenantCacheServiceMock = new Mock<ITenantCacheServices>();
            var httpAccessor = new Mock<IHttpContextAccessor>();

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(ITenantCacheServices)))
                .Returns(tenantCacheServiceMock.Object);

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(IHttpContextAccessor)))
                .Returns(httpAccessor.Object);

            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            var scopeFactoryMock = _mocker.GetMock<IServiceScopeFactory>();
            scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            tenantCacheServiceMock
                .Setup(x => x.FindTenantAsync(tenantName))
                .ReturnsAsync(tenant);

            // Act
            await _sut.FindConnectionStringAndHttpAcessorAsync(tenantName);

            // Assert
            scopeMock.Verify(x => x.Dispose(), Times.Once);
        }

        #endregion
    }
}
