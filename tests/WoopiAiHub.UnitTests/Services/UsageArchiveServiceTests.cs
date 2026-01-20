using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class UsageArchiveServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly UsageArchiveService _service;
        private readonly Mock<IMarketPlaceApi> _marketPlaceApiMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<ILogger<UsageArchiveService>> _loggerMock;

        public UsageArchiveServiceTests()
        {
            var configData = new Dictionary<string, string?>
            {
                { "KeyAccess", "test-key" },
                { "ConnectionStrings:TemplateConnection", "Server=localhost;Database=___NEWDB___;" },
                { "UsageManagement:ArchiveMonthsThreshold", "3" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData!)
                .Build();

            _marketPlaceApiMock = new Mock<IMarketPlaceApi>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _loggerMock = new Mock<ILogger<UsageArchiveService>>();

            _mocker = new AutoMocker();
            _mocker.Use(_marketPlaceApiMock);
            _mocker.Use(_scopeFactoryMock);
            _mocker.Use(_loggerMock);

            // Setup configuration
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["KeyAccess"]).Returns("test-key");
            // Mock GetConnectionString
            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(s => s["TemplateConnection"]).Returns("Server=localhost;Database=___NEWDB___;");
            configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionSection.Object);

            // Mock GetValue<int>
            var mockThresholdSection = new Mock<IConfigurationSection>();
            mockThresholdSection.Setup(s => s.Value).Returns("3");
            configMock.Setup(c => c.GetSection("UsageManagement:ArchiveMonthsThreshold"))
                .Returns(mockThresholdSection.Object);
            _mocker.Use(configMock.Object);

            _service = _mocker.CreateInstance<UsageArchiveService>();
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should return early when no tenants found")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_NoTenantsFound_ReturnsEarly()
        {
            // Arrange
            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(new List<TenantListDto>());

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub), Times.Once);
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should process each tenant when tenants exist")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_TenantsExist_ProcessesEachTenant()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" },
                new TenantListDto { Name = "Tenant2", DatabaseName = "DB2" }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockSubscriptionPeriodService = new Mock<ISubscriptionPeriodServices>();
            var mockUsageMonthRepository = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository)))
                .Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            mockServiceProvider.Setup(x => x.GetService(typeof(ISubscriptionPeriodServices)))
                .Returns(mockSubscriptionPeriodService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository)))
                .Returns(mockUsageMonthRepository.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub), Times.Once);
            _mocker.GetMock<IServiceScopeFactory>()
                .Verify(x => x.CreateScope(), Times.Exactly(4));
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should throw when KeyAccess not configured")]
        [Trait("Archive", "Error")]
        public async Task ArchiveOldUsageAsync_KeyAccessNotConfigured_ThrowsException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                { "ConnectionStrings:TemplateConnection", "Server=localhost;Database=___NEWDB___;" },
                { "UsageManagement:ArchiveMonthsThreshold", "3" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var service = new UsageArchiveService(
                _marketPlaceApiMock.Object,
                configuration,
                _scopeFactoryMock.Object,
                _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ArchiveOldUsageAsync());
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should archive and delete old records")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_WithOldRecords_ArchivesAndDeletes()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" }
            };

            var cutoffDate = DateTime.UtcNow.AddMonths(-3);
            var oldRecords = new List<UsageDaily>
            {
                new UsageDaily(1, cutoffDate.AddDays(-10), Guid.NewGuid(), 1, 100, true, 1),
                new UsageDaily(2, cutoffDate.AddDays(-5), Guid.NewGuid(), 1, 50, true, 1)
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockSubscriptionPeriodService = new Mock<ISubscriptionPeriodServices>();
            var mockUsageMonthRepository = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(oldRecords);
            mockUsageLogRepo.Setup(x =>
                    x.ExistsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockSubscriptionPeriodService.Setup(x => x.FindLastUnprocessedAsync())
                .ReturnsAsync((SubscriptionPeriod?)null);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository)))
                .Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubscriptionPeriodServices)))
                .Returns(mockSubscriptionPeriodService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository)))
                .Returns(mockUsageMonthRepository.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()), Times.Once);
            mockUsageLogRepo.Verify(
                x => x.BulkInsertAsync(It.Is<List<UsageLog>>(logs => logs.Count == oldRecords.Count),
                    It.IsAny<CancellationToken>()), Times.Once);
            mockUsageDailyRepo.Verify(
                x => x.BulkDeleteAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == oldRecords.Count)), Times.Once);
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should not archive or delete when no old records")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_NoOldRecords_DoesNotArchiveOrDelete()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockSubscriptionPeriodService = new Mock<ISubscriptionPeriodServices>();
            var mockUsageMonthRepository = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository)))
                .Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubscriptionPeriodServices)))
                .Returns(mockSubscriptionPeriodService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository)))
                .Returns(mockUsageMonthRepository.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()), Times.Once);
            mockUsageLogRepo.Verify(x => x.BulkInsertAsync(It.IsAny<List<UsageLog>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            mockUsageDailyRepo.Verify(x => x.BulkDeleteAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should not send consumption where there's no unprocessed period")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_NoUnprocessedPeriod_DoesnotSendConsunption()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" }
            };

            _mocker.GetMock<IMarketPlaceApi>()
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockSubscriptionPeriodService = new Mock<ISubscriptionPeriodServices>();
            var mockUsageMonthRepository = new Mock<IUsageMonthRepository>();

            mockSubscriptionPeriodService.Setup(x => x.FindLastUnprocessedAsync())
                .ReturnsAsync((SubscriptionPeriod?)null);

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository)))
                .Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubscriptionPeriodServices)))
                .Returns(mockSubscriptionPeriodService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository)))
                .Returns(mockUsageMonthRepository.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()), Times.Once);
            mockUsageLogRepo.Verify(x => x.BulkInsertAsync(It.IsAny<List<UsageLog>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            mockUsageDailyRepo.Verify(x => x.BulkDeleteAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Theory(DisplayName = "ArchiveOldUsageAsync should send consumption where there's unprocessed period")]
        [Trait("Archive", "Success")]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ArchiveOldUsageAsync_UnprocessedPeriod_SendConsunption(bool sendStatus)
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" }
            };

            var subscriptionPeriod = new SubscriptionPeriod
            (
                DateTime.UtcNow.AddMonths(-1),
                DateTime.UtcNow,
                false
            );

            _mocker.GetMock<IMarketPlaceApi>()
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockSubscriptionPeriodService = new Mock<ISubscriptionPeriodServices>();
            var mockUsageMonthRepository = new Mock<IUsageMonthRepository>();
            var mockMarketPlaceApi = _mocker.GetMock<IMarketPlaceApi>();

            mockMarketPlaceApi
                .Setup(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()))
                .ReturnsAsync(sendStatus);
            mockSubscriptionPeriodService.Setup(x => x.FindLastUnprocessedAsync())
                .ReturnsAsync(subscriptionPeriod);
            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());
            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository)))
                .Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);
            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubscriptionPeriodServices)))
                .Returns(mockSubscriptionPeriodService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository)))
                .Returns(mockUsageMonthRepository.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()), Times.Once);
            mockUsageLogRepo.Verify(x => x.BulkInsertAsync(It.IsAny<List<UsageLog>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            mockUsageDailyRepo.Verify(x => x.BulkDeleteAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should not duplicate records in log")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_RecordAlreadyExists_DoesNotDuplicate()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "DB1" }
            };

            var cutoffDate = DateTime.UtcNow.AddMonths(-3);
            var oldRecords = new List<UsageDaily>
            {
                new UsageDaily(1, cutoffDate.AddDays(-10), Guid.NewGuid(), 1, 100, true, 1),
                new UsageDaily(2, cutoffDate.AddDays(-5), Guid.NewGuid(), 1, 50, true, 1)
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockSubscriptionPeriodService = new Mock<ISubscriptionPeriodServices>();
            var mockUsageMonthRepository = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(oldRecords);

            // First record already exists in log
            mockUsageLogRepo.Setup(x =>
                    x.ExistsAsync(It.Is<int>(id => id == 1), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mockUsageLogRepo
                .Setup(x => x.ExistsAsync(It.Is<int>(id => id == 2), It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(false);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository)))
                .Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ISubscriptionPeriodServices)))
                .Returns(mockSubscriptionPeriodService.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository)))
                .Returns(mockUsageMonthRepository.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageLogRepo.Verify(
                x => x.BulkInsertAsync(It.Is<List<UsageLog>>(logs => logs.Count == 1), It.IsAny<CancellationToken>()),
                Times.Once);
            mockUsageDailyRepo.Verify(x => x.BulkDeleteAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == 2)),
                Times.Once);
        }

        [Fact(DisplayName = "Resilience pipeline should retry on failure")]
        [Trait("Resilience", "Retry")]
        public async Task ResiliencePipeline_OnFailure_Retries()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto 
                { 
                    Name = "Tenant1", 
                    DatabaseName = "DB1",
                    DateStart = DateTime.UtcNow.AddMonths(-2),
                    DateEnd = DateTime.UtcNow.AddDays(-1)
                }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockUsageMonthRepo = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.SetupProperty(x => x.HttpContext, null);
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());
            mockUsageMonthRepo.Setup(x => x.FindTotalUsageAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(100);

            _marketPlaceApiMock
                .SetupSequence(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()))
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ReturnsAsync(true);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()), Times.Exactly(3));
        }

        [Fact(DisplayName = "Resilience pipeline circuit breaker should open after failures")]
        [Trait("Resilience", "CircuitBreaker")]
        public async Task ResiliencePipeline_CircuitBreaker_OpensAfterFailures()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto 
                { 
                    Name = "Tenant1", 
                    DatabaseName = "DB1",
                    DateStart = DateTime.UtcNow.AddMonths(-2),
                    DateEnd = DateTime.UtcNow.AddDays(-1)
                }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();
            var mockUsageMonthRepo = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.SetupProperty(x => x.HttpContext, null);
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());
            mockUsageMonthRepo.Setup(x => x.FindTotalUsageAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(100);

            _marketPlaceApiMock
                .Setup(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()))
                .ThrowsAsync(new HttpRequestException("Service unavailable"));

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.ArchiveOldUsageAsync());
        }

        [Fact(DisplayName = "Scope factory should create scope and set connection string")]
        [Trait("Scope", "Success")]
        public async Task ScopeFactory_CreatesScope_AndSetsConnectionString()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto { Name = "Tenant1", DatabaseName = "TestDB" }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();

            var httpContext = new DefaultHttpContext();
            mockHttpAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(new Mock<IUsageLogRepository>().Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
            Assert.NotNull(httpContext.Items["TenantConnection"]);
            Assert.Contains("TestDB", httpContext.Items["TenantConnection"]!.ToString()!);
        }

        [Fact(DisplayName = "SendMonthlyUsageIfExpiredAsync should return early when subscription is active")]
        [Trait("SendMonthlyUsage", "EarlyExit")]
        public async Task SendMonthlyUsageIfExpiredAsync_SubscriptionActive_ReturnsEarly()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto 
                { 
                    Name = "Tenant1", 
                    DatabaseName = "DB1",
                    DateStart = DateTime.UtcNow.AddMonths(-1),
                    DateEnd = DateTime.UtcNow.AddDays(30)
                }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(new Mock<IUsageLogRepository>().Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()), Times.Never);
        }

        [Fact(DisplayName = "SendMonthlyUsageIfExpiredAsync should send usage when subscription expired")]
        [Trait("SendMonthlyUsage", "Success")]
        public async Task SendMonthlyUsageIfExpiredAsync_SubscriptionExpired_SendsUsage()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto 
                { 
                    Name = "Tenant1", 
                    DatabaseName = "DB1",
                    DateStart = DateTime.UtcNow.AddMonths(-2),
                    DateEnd = DateTime.UtcNow.AddDays(-1)
                }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageMonthRepo = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.SetupProperty(x => x.HttpContext, null);
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());
            mockUsageMonthRepo.Setup(x => x.FindTotalUsageAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(150);

            _marketPlaceApiMock
                .Setup(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()))
                .ReturnsAsync(true);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(new Mock<IUsageLogRepository>().Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.ProcessConsumption(
                    "test-key", 
                    It.Is<ExcessManagementTenantDto>(dto => dto.Tenant == "Tenant1" && dto.UsageCount == 150)), 
                    Times.Once);
        }

        [Fact(DisplayName = "SendMonthlyUsageIfExpiredAsync should return early when no usage found")]
        [Trait("SendMonthlyUsage", "EarlyExit")]
        public async Task SendMonthlyUsageIfExpiredAsync_NoUsage_ReturnsEarly()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto 
                { 
                    Name = "Tenant1", 
                    DatabaseName = "DB1",
                    DateStart = DateTime.UtcNow.AddMonths(-2),
                    DateEnd = DateTime.UtcNow.AddDays(-1)
                }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageMonthRepo = new Mock<IUsageMonthRepository>();

            mockHttpAccessor.SetupProperty(x => x.HttpContext, null);
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());
            mockUsageMonthRepo.Setup(x => x.FindTotalUsageAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(new Mock<IUsageLogRepository>().Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageMonthRepository))).Returns(mockUsageMonthRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()), Times.Never);
        }

        [Fact(DisplayName = "SendMonthlyUsageIfExpiredAsync should return early when DateEnd is null")]
        [Trait("SendMonthlyUsage", "EarlyExit")]
        public async Task SendMonthlyUsageIfExpiredAsync_DateEndNull_ReturnsEarly()
        {
            // Arrange
            var tenants = new List<TenantListDto>
            {
                new TenantListDto 
                { 
                    Name = "Tenant1", 
                    DatabaseName = "DB1",
                    DateStart = DateTime.UtcNow.AddMonths(-1),
                    DateEnd = null
                }
            };

            _marketPlaceApiMock
                .Setup(x => x.FindAllTenantsByModuleAsync("test-key", ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(new Mock<IUsageLogRepository>().Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _marketPlaceApiMock
                .Verify(x => x.ProcessConsumption(It.IsAny<string>(), It.IsAny<ExcessManagementTenantDto>()), Times.Never);
        }
    }
}
