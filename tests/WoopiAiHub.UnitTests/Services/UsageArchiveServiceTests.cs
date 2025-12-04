using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using WoopiAiHub.Application.Services;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using Xunit;

namespace WoopiAiHub.UnitTests.Services
{
    public class UsageArchiveServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly UsageArchiveService _service;

        public UsageArchiveServiceTests()
        {
            _mocker = new AutoMocker();

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
            configMock.Setup(c => c.GetSection("UsageManagement:ArchiveMonthsThreshold")).Returns(mockThresholdSection.Object);
            _mocker.Use(configMock.Object);

            _service = _mocker.CreateInstance<UsageArchiveService>();
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should return early when no tenants found")]
        [Trait("Archive", "Success")]
        public async Task ArchiveOldUsageAsync_NoTenantsFound_ReturnsEarly()
        {
            // Arrange
            _mocker.GetMock<ITenantCacheServices>()
                .Setup(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub))
                .ReturnsAsync(new List<TenantListDto>());

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _mocker.GetMock<ITenantCacheServices>()
                .Verify(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub), Times.Once);
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

            _mocker.GetMock<ITenantCacheServices>()
                .Setup(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            _mocker.GetMock<ITenantCacheServices>()
                .Verify(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub), Times.Once);
            _mocker.GetMock<IServiceScopeFactory>()
                .Verify(x => x.CreateScope(), Times.Exactly(tenants.Count));
        }

        [Fact(DisplayName = "ArchiveOldUsageAsync should throw when KeyAccess not configured")]
        [Trait("Archive", "Error")]
        public async Task ArchiveOldUsageAsync_KeyAccessNotConfigured_ThrowsException()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["KeyAccess"]).Returns((string)null!);
            // Mock GetConnectionString
            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(s => s["TemplateConnection"]).Returns("Server=localhost;Database=___NEWDB___;");
            configMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockConnectionSection.Object);
            _mocker.Use(configMock.Object);

            var service = _mocker.CreateInstance<UsageArchiveService>();

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

            _mocker.GetMock<ITenantCacheServices>()
                .Setup(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(oldRecords);
            mockUsageLogRepo.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()), Times.Once);
            mockUsageLogRepo.Verify(x => x.BulkInsertAsync(It.Is<List<UsageLog>>(logs => logs.Count == oldRecords.Count), It.IsAny<CancellationToken>()), Times.Once);
            mockUsageDailyRepo.Verify(x => x.BulkDeleteAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == oldRecords.Count)), Times.Once);
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

            _mocker.GetMock<ITenantCacheServices>()
                .Setup(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<UsageDaily>());

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageDailyRepo.Verify(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()), Times.Once);
            mockUsageLogRepo.Verify(x => x.BulkInsertAsync(It.IsAny<List<UsageLog>>(), It.IsAny<CancellationToken>()), Times.Never);
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

            _mocker.GetMock<ITenantCacheServices>()
                .Setup(x => x.FindAllTenantsAsync(ColTypeModule.WoopiAiHub))
                .ReturnsAsync(tenants);

            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var mockUsageDailyRepo = new Mock<IUsageDailyRepository>();
            var mockUsageLogRepo = new Mock<IUsageLogRepository>();

            mockHttpAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            mockUsageDailyRepo.Setup(x => x.FindOldRecordsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(oldRecords);

            // First record already exists in log
            mockUsageLogRepo.Setup(x => x.ExistsAsync(It.Is<int>(id => id == 1), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            mockUsageLogRepo.Setup(x => x.ExistsAsync(It.Is<int>(id => id == 2), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            mockServiceProvider.Setup(x => x.GetService(typeof(IHttpContextAccessor))).Returns(mockHttpAccessor.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageDailyRepository))).Returns(mockUsageDailyRepo.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IUsageLogRepository))).Returns(mockUsageLogRepo.Object);

            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>().Setup(x => x.CreateScope()).Returns(mockScope.Object);

            // Act
            await _service.ArchiveOldUsageAsync();

            // Assert
            mockUsageLogRepo.Verify(x => x.BulkInsertAsync(It.Is<List<UsageLog>>(logs => logs.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
            mockUsageDailyRepo.Verify(x => x.BulkDeleteAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == 2)), Times.Once);
        }
    }
}
