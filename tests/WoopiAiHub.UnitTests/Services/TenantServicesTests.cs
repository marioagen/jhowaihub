using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
using System.Data.Common;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    [Collection(nameof(TenantCollection))]
    public class TenantServicesTests
    {
        private readonly AutoMocker _mocker;
        private readonly TenantServices _tenantServices;

        public TenantServicesTests(TenantFixture tenantFixture)
        {
            _mocker = new AutoMocker();

            var mockConfSectionTemplate = new Mock<IConfigurationSection>();
            mockConfSectionTemplate.SetupGet(m => m[It.Is<string>(s => s == "TemplateConnection")]).Returns("mock value");

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(mockConfSectionTemplate.Object);
            configMock.Setup(config => config[It.Is<string>(s => s == "keyAccess")]).Returns("mockKeyAccess");
            configMock.Setup(x => x.GetSection("KeyAccess").Value).Returns(Guid.NewGuid().ToString());
            configMock.Setup(c => c["OCRSettings:OCRModel"]).Returns("mock value");

            _mocker.Use(configMock);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x.Items["Tenant"]).Returns(It.IsAny<TenantInfoDto>());
            _mocker.Use(httpContext.Object);

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupAllProperties();
            httpContextAccessor.Object.HttpContext = new DefaultHttpContext();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);
            _mocker.Use(httpContextAccessor.Object);

            // Mock de ICoreDependencies
            var coreDependenciesMock = new Mock<ICoreDependencies>();
            coreDependenciesMock.SetupGet(x => x.Configuration).Returns(configMock.Object);
            coreDependenciesMock.SetupGet(x => x.HttpContextAccessor).Returns(httpContextAccessor.Object);
            _mocker.Use(coreDependenciesMock.Object);

            // Mock de IApiDependencies
            var apiDependenciesMock = new Mock<IApiDependencies>();
            apiDependenciesMock.SetupGet(x => x.MarketPlaceApi).Returns(_mocker.GetMock<IMarketPlaceApi>().Object);
            apiDependenciesMock.SetupGet(x => x.KeyGeneratorApi).Returns(_mocker.GetMock<IKeyGeneratorApi>().Object);
            _mocker.Use(apiDependenciesMock.Object);

            var _serviceProviderMock = new Mock<IServiceProvider>();
            var _userServiceMock = new Mock<IUserServices>();
            var _dbConnectionMock = new Mock<DbConnection>();

            // 1. Configurar o DbContext Real com Conexão Mockada
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_dbConnectionMock.Object)
                .Options;

            // Se o seu ApplicationDbContext tiver um construtor que aceita Options
            var _dbContext = new ApplicationDbContext(options);

            // 2. Configurar a Hierarquia de Escopo do ServiceProvider
            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();

            // O provedor do escopo deve retornar o DbContext e o UserService
            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(_dbContext);

            scopeServiceProviderMock
                .Setup(x => x.GetService(typeof(IUserServices)))
                .Returns(_userServiceMock.Object);

            scopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock
                .Setup(x => x.CreateScope())
                .Returns(scopeMock.Object);

            _serviceProviderMock
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopeFactoryMock.Object);

            _mocker.Use(_serviceProviderMock.Object);

            _tenantServices = _mocker.CreateInstance<TenantServices>();
        }

        [Fact(DisplayName = "Test FindAllByUserEmail returns tenants for valid email")]
        [Trait("Find", "Success")]
        public async Task FindAllByUserEmail_Success()
        {
            // Arrange
            var email = "test@example.com";
            var expectedTenants = new List<string> { "Tenant1", "Tenant2" };

            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApiMock
                .Setup(api => api.FindTenantsByUserEmail(It.IsAny<string>(), email))
                .ReturnsAsync(expectedTenants);

            // Act
            var result = await _tenantServices.FindAllByUserEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTenants, result);

            marketPlaceApiMock.Verify(api => api.FindTenantsByUserEmail("mockKeyAccess", email), Times.Once);
        }

        [Fact(DisplayName = "InitializeTenant throws ArgumentException for null tenant")]
        [Trait("Initialize", "Error")]
        public async Task InitializeTenant_NullTenant_ThrowsArgumentException()
        {
            // Arrange
            string tenant = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _tenantServices.InitializeTenant(tenant));
            Assert.Equal("Tenant name cannot be null or empty. (Parameter 'tenant')", exception.Message);
            Assert.Equal("tenant", exception.ParamName);
        }

        [Fact(DisplayName = "InitializeTenant throws InvalidOperationException when KeyAccess is not configured")]
        [Trait("Initialize", "Error")]
        public async Task InitializeTenant_KeyAccessNotConfigured_ThrowsInvalidOperationException()
        {
            // Arrange
            var tenant = "TestTenant";

            // Mock para retornar null para KeyAccess
            _mocker.GetMock<IConfiguration>()
                   .Setup(c => c.GetSection("KeyAccess").Value).Returns((string)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _tenantServices.InitializeTenant(tenant));
            Assert.Equal("KeyAccess is not configured in the application settings.", exception.Message);
        }

        [Fact(DisplayName = "ProcessSubscription should create database and notify marketplace when subscription is valid")]
        [Trait("ProcessSubscription", "Success")]
        public void ProcessSubscription_ShouldCreateTenantSuccessfully()
        {
            // Arrange
            var tenantSubscriptionDto = new TenantSubscriptionDto
            {
                MarketplaceId = Guid.NewGuid(),
                Name = "tenant-name",
                Email = "owner@example.com",
                IsActive = true,
                PlanName = "basic",
                DataBaseName = "tenant_db",
                DateStart = DateTime.UtcNow
            };

            var tenantRepositoryMock = _mocker.GetMock<ITenantRepository>();
            tenantRepositoryMock
                .Setup(r => r.CreateDatabase())
                .Returns(true);


            var marketPlaceApiMock = _mocker.GetMock<IMarketPlaceApi>();
            marketPlaceApiMock
                .Setup(m => m.SendDatabaseCreatedNotification(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            _tenantServices.ProcessSubscription(tenantSubscriptionDto);

            // Assert
            tenantRepositoryMock.Verify(r => r.CreateDatabase(), Times.Once);
            marketPlaceApiMock.Verify(m => m.SendDatabaseCreatedNotification(It.IsAny<string>(), tenantSubscriptionDto.Name), Times.Once);
        }

    }
}
