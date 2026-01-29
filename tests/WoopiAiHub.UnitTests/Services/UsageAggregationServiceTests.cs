using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class UsageAggregationServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly UsageAggregationService _service;

        public UsageAggregationServiceTests()
        {
            _mocker = new AutoMocker();

            // Setup configuration
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["KeyAccess"]).Returns("test-key");

            // Mock GetConnectionString
            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(s => s["TemplateConnection"]).Returns("Server=localhost;Database=___NEWDB___;");
            configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionSection.Object);

            _mocker.Use(configMock.Object);

            _service = _mocker.CreateInstance<UsageAggregationService>();
        }

        [Fact(DisplayName = "ProcessUnprocessedUsageAsync should return early when no tenants found")]
        [Trait("Process", "Success")]
        public async Task ProcessUnprocessedUsageAsync_NoTenantsFound_ReturnsEarly()
        {
            // Arrange
            _mocker.GetMock<IMarketPlaceApi>()
                .Setup(x => x.FindAllTenantsByModuleAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(new List<TenantListDto>());

            // Act
            await _service.ProcessUnprocessedUsageAsync();

            // Assert
            _mocker.GetMock<IMarketPlaceApi>()
                .Verify(x => x.FindAllTenantsByModuleAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()), Times.Once);
        }

        [Fact(DisplayName = "ProcessUnprocessedUsageAsync should process each tenant when tenants exist")]
        [Trait("Process", "Success")]
        public async Task ProcessUnprocessedUsageAsync_TenantsExist_ProcessesEachTenant()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" },
                new TenantListDto { Name = "Tenant2", DatabaseName = "DB2" }
            };

            _mocker.GetMock<IMarketPlaceApi>()
                .Setup(x => x.FindAllTenantsByModuleAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageMonthRepo = new Mock<IUsageMonthRepository>();

            // Mock IConfiguration in the scope
            var mockScopeConfig = new Mock<IConfiguration>();
            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(s => s["TemplateConnection"]).Returns("Server=localhost;Database=___NEWDB___;");
            mockScopeConfig.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionSection.Object);

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindUnprocessedAsync()).ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockScopeConfig.Object);

            // Mock ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestDb;ConnectRetryCount=0")
                .Options;
            var realDbContext = new ApplicationDbContext(mockHttpAccessor.Object, options);
            mockServiceProvider.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(realDbContext);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ProcessUnprocessedUsageAsync();

            // Assert
            _mocker.GetMock<IMarketPlaceApi>()
                .Verify(x => x.FindAllTenantsByModuleAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()), Times.Once);
            _mocker.GetMock<IServiceScopeFactory>()
                .Verify(x => x.CreateScope(), Times.Exactly(tenants.Count));
        }

        [Fact(DisplayName = "ProcessUnprocessedUsageAsync should throw when KeyAccess not configured")]
        [Trait("Process", "Error")]
        public async Task ProcessUnprocessedUsageAsync_KeyAccessNotConfigured_ThrowsException()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["KeyAccess"]).Returns((string)null!);

            // Mock GetConnectionString
            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(s => s["TemplateConnection"]).Returns("Server=localhost;Database=___NEWDB___;");
            configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionSection.Object);

            _mocker.Use(configMock.Object);

            var service = _mocker.CreateInstance<UsageAggregationService>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessUnprocessedUsageAsync());
        }

        [Fact(DisplayName = "ProcessUnprocessedUsageAsync should process and mark records as processed")]
        [Trait("Process", "Success")]
        public async Task ProcessUnprocessedUsageAsync_WithUnprocessedRecords_ProcessesAndMarks()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" }
            };

            var unprocessedRecords = new List<UsageDaily>
            {
                new UsageDaily(1, DateTime.UtcNow, Guid.NewGuid(), 1, 100, false, 1),
                new UsageDaily(2, DateTime.UtcNow, Guid.NewGuid(), 1, 50, false, 1)
            };

            _mocker.GetMock<IMarketPlaceApi>()
                .Setup(x => x.FindAllTenantsByModuleAsync(It.IsAny<string>(), It.IsAny<ColTypeModule>()))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageMonthRepo = new Mock<IUsageMonthRepository>();

            // Mock IConfiguration in the scope
            var mockScopeConfig = new Mock<IConfiguration>();
            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(s => s["TemplateConnection"]).Returns("Server=localhost;Database=___NEWDB___;");
            mockScopeConfig.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionSection.Object);

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindUnprocessedAsync()).ReturnsAsync(unprocessedRecords);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IConfiguration))).Returns(mockScopeConfig.Object);

            // Mock ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestDb;ConnectRetryCount=0")
                .Options;
            var realDbContext = new ApplicationDbContext(mockHttpAccessor.Object, options);
            mockServiceProvider.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(realDbContext);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ProcessUnprocessedUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindUnprocessedAsync(), Times.Once);
            mockUsageMonthRepo.Verify(x => x.UpsertAsync(It.IsAny<UsageMonth>()), Times.AtLeastOnce);
            mockUsageDailyRepo.Verify(x => x.MarkAsProcessedAsync(It.IsAny<IEnumerable<int>>()), Times.Once);
        }
    }
}
