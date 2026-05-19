using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Repository.Cache;
using Xunit;

namespace WoopiAiHub.UnitTests.Cache
{
    public class UserTenantAccessCacheServiceTests
    {
        private readonly Mock<IMarketPlaceApi> _marketplaceMock;
        private readonly IConfiguration _configuration;
        private readonly MemoryDistributedCache _cache;
        private readonly UserTenantAccessCacheService _service;

        public UserTenantAccessCacheServiceTests()
        {
            _marketplaceMock = new Mock<IMarketPlaceApi>();
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["KeyAccess"] = "test-key",
                    ["Cache:UserTenantAccessExpirationMinutes"] = "10"
                })
                .Build();

            _cache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(
                new MemoryDistributedCacheOptions()));

            _service = new UserTenantAccessCacheService(
                _cache,
                _marketplaceMock.Object,
                _configuration,
                Mock.Of<ILogger<UserTenantAccessCacheService>>());
        }

        [Fact(DisplayName = "Loads tenants from marketplace on cache miss")]
        [Trait("FindAllowedTenantsByEmailAsync", "Success")]
        public async Task FindAllowedTenantsByEmailAsync_CacheMiss_CallsMarketplace()
        {
            // Arrange
            var tenants = new List<TenantAccessDto> { new("Tenant1", true) };
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub("test-key", "user@test.com"))
                .ReturnsAsync(new ResponseCheckAccessDto
                {
                    HasAccess = true,
                    Tenants = tenants
                });

            // Act
            var result = await _service.FindAllowedTenantsByEmailAsync("user@test.com");

            // Assert
            Assert.Single(result);
            Assert.Equal("Tenant1", result[0].Name);
            _marketplaceMock.Verify(m => m.CheckAccessByHub("test-key", "user@test.com"), Times.Once);
        }

        [Fact(DisplayName = "Returns cached tenants without calling marketplace")]
        [Trait("FindAllowedTenantsByEmailAsync", "Success")]
        public async Task FindAllowedTenantsByEmailAsync_CacheHit_SkipsMarketplace()
        {
            // Arrange
            var tenants = new List<TenantAccessDto> { new("Tenant1", true) };
            var cacheKey = "user-tenants:user@test.com";
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenants));

            // Act
            var result = await _service.FindAllowedTenantsByEmailAsync("user@test.com");

            // Assert
            Assert.Single(result);
            _marketplaceMock.Verify(m => m.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact(DisplayName = "Returns empty list when user has no marketplace access")]
        [Trait("FindAllowedTenantsByEmailAsync", "Success")]
        public async Task FindAllowedTenantsByEmailAsync_NoAccess_ReturnsEmpty()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub("test-key", "user@test.com"))
                .ReturnsAsync(new ResponseCheckAccessDto { HasAccess = false });

            // Act
            var result = await _service.FindAllowedTenantsByEmailAsync("user@test.com");

            // Assert
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Throws when marketplace call fails")]
        [Trait("FindAllowedTenantsByEmailAsync", "Fail")]
        public async Task FindAllowedTenantsByEmailAsync_MarketplaceThrows_ThrowsInvalidOperation()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("unavailable"));

            // Act / Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.FindAllowedTenantsByEmailAsync("user@test.com"));
        }

        [Fact(DisplayName = "IsTenantAllowedForUserAsync returns true when tenant is in list")]
        [Trait("IsTenantAllowedForUserAsync", "Success")]
        public async Task IsTenantAllowedForUserAsync_TenantInList_ReturnsTrue()
        {
            // Arrange
            _marketplaceMock
                .Setup(m => m.CheckAccessByHub("test-key", "user@test.com"))
                .ReturnsAsync(new ResponseCheckAccessDto
                {
                    HasAccess = true,
                    Tenants = new List<TenantAccessDto> { new("Tenant1", true) }
                });

            // Act
            var result = await _service.IsTenantAllowedForUserAsync("user@test.com", "Tenant1");

            // Assert
            Assert.True(result);
        }
    }
}
