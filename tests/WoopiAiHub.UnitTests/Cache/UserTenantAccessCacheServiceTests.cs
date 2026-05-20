using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Repository.Cache;
using WoopiAiHub.UnitTests.Fixture;
using Xunit;

namespace WoopiAiHub.UnitTests.Cache
{
    [Collection(nameof(TenantCollection))]
    public class UserTenantAccessCacheServiceTests
    {
        private const string TestKeyAccess = "test-key";
        private readonly Mock<IMarketPlaceApi> _marketplaceMock;
        private readonly MemoryDistributedCache _cache;
        private readonly UserTenantAccessCacheService _service;

        public UserTenantAccessCacheServiceTests()
        {
            _marketplaceMock = new Mock<IMarketPlaceApi>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["KeyAccess"] = TestKeyAccess,
                    ["Cache:UserTenantAccessExpirationMinutes"] = "10"
                })
                .Build();

            _cache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(
                new MemoryDistributedCacheOptions()));

            _service = new UserTenantAccessCacheService(
                _cache,
                _marketplaceMock.Object,
                configuration,
                Mock.Of<ILogger<UserTenantAccessCacheService>>());
        }

        [Fact(DisplayName = "Tests FindAllowedTenantsByEmailAsync and calls marketplace on cache miss")]
        [Trait("FindAllowedTenantsByEmailAsync", "Success")]
        public async Task FindAllowedTenantsByEmailAsync_CacheMiss_CallsMarketplace()
        {
            // Arrange
            var tenants = TenantFixture.FindValidTenantAccessList();
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub(TestKeyAccess, TenantFixture.ValidUserEmail))
                .ReturnsAsync(TenantFixture.FindValidResponseCheckAccessDto(tenants: tenants));

            // Act
            var result = await _service.FindAllowedTenantsByEmailAsync(TenantFixture.ValidUserEmail);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Tenant1", result[0].Name);
            _marketplaceMock.Verify(
                m => m.CheckAccessByHub(TestKeyAccess, TenantFixture.ValidUserEmail),
                Times.Once);
        }

        [Fact(DisplayName = "Tests FindAllowedTenantsByEmailAsync and returns cached tenants without calling marketplace")]
        [Trait("FindAllowedTenantsByEmailAsync", "Success")]
        public async Task FindAllowedTenantsByEmailAsync_CacheHit_SkipsMarketplace()
        {
            // Arrange
            var tenants = TenantFixture.FindValidTenantAccessList();
            var cacheKey = $"user-tenants:{TenantFixture.ValidUserEmail}";
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenants));

            // Act
            var result = await _service.FindAllowedTenantsByEmailAsync(TenantFixture.ValidUserEmail);

            // Assert
            Assert.Equal(2, result.Count);
            _marketplaceMock.Verify(
                m => m.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact(DisplayName = "Tests FindAllowedTenantsByEmailAsync and returns empty list when user has no marketplace access")]
        [Trait("FindAllowedTenantsByEmailAsync", "Success")]
        public async Task FindAllowedTenantsByEmailAsync_NoAccess_ReturnsEmpty()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub(TestKeyAccess, TenantFixture.ValidUserEmail))
                .ReturnsAsync(TenantFixture.FindValidResponseCheckAccessDto(hasAccess: false));

            // Act
            var result = await _service.FindAllowedTenantsByEmailAsync(TenantFixture.ValidUserEmail);

            // Assert
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Tests FindAllowedTenantsByEmailAsync and throws when marketplace call fails")]
        [Trait("FindAllowedTenantsByEmailAsync", "Fail")]
        public async Task FindAllowedTenantsByEmailAsync_MarketplaceThrows_ThrowsInvalidOperation()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.FindAllowedTenantsByEmailAsync(TenantFixture.ValidUserEmail));
        }

        [Fact(DisplayName = "Tests FindAllowedTenantsByEmailAsync and throws when KeyAccess is not configured")]
        [Trait("FindAllowedTenantsByEmailAsync", "Fail")]
        public async Task FindAllowedTenantsByEmailAsync_KeyAccessMissing_ThrowsInvalidOperation()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();
            var service = new UserTenantAccessCacheService(
                _cache,
                _marketplaceMock.Object,
                configuration,
                Mock.Of<ILogger<UserTenantAccessCacheService>>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.FindAllowedTenantsByEmailAsync(TenantFixture.ValidUserEmail));
        }

        [Fact(DisplayName = "Tests IsTenantAllowedForUserAsync and returns true when tenant is in list")]
        [Trait("IsTenantAllowedForUserAsync", "Success")]
        public async Task IsTenantAllowedForUserAsync_TenantInList_ReturnsTrue()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub(TestKeyAccess, TenantFixture.ValidUserEmail))
                .ReturnsAsync(TenantFixture.FindValidResponseCheckAccessDto());

            // Act
            var result = await _service.IsTenantAllowedForUserAsync(TenantFixture.ValidUserEmail, "Tenant1");

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "Tests IsTenantAllowedForUserAsync and returns false when tenant is not in list")]
        [Trait("IsTenantAllowedForUserAsync", "Success")]
        public async Task IsTenantAllowedForUserAsync_TenantNotInList_ReturnsFalse()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub(TestKeyAccess, TenantFixture.ValidUserEmail))
                .ReturnsAsync(TenantFixture.FindValidResponseCheckAccessDto());

            // Act
            var result = await _service.IsTenantAllowedForUserAsync(TenantFixture.ValidUserEmail, "Unknown");

            // Assert
            Assert.False(result);
        }
    }
}
