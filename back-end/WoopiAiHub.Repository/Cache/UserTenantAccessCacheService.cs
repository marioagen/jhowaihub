using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;

namespace WoopiAiHub.Repository.Cache
{
    public class UserTenantAccessCacheService : IUserTenantAccessCacheServices
    {
        private const string CacheKeyPrefix = "user-tenants:";
        private readonly IDistributedCache _cache;
        private readonly IMarketPlaceApi _marketplace;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserTenantAccessCacheService> _logger;

        public UserTenantAccessCacheService(
            IDistributedCache cache,
            IMarketPlaceApi marketplace,
            IConfiguration configuration,
            ILogger<UserTenantAccessCacheService> logger)
        {
            _cache = cache;
            _marketplace = marketplace;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Returns tenants the user is allowed to access for the Hub module.
        /// </summary>
        /// <param name="email">User email used for marketplace lookup.</param>
        /// <returns>Allowed tenants; empty when the user has no access.</returns>
        /// <exception cref="InvalidOperationException">When the marketplace call fails (fail-closed for runtime validation).</exception>
        public async Task<IReadOnlyList<TenantAccessDto>> FindAllowedTenantsByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var cacheKey = $"{CacheKeyPrefix}{normalizedEmail}";
            var cached = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrWhiteSpace(cached))
                return JsonSerializer.Deserialize<List<TenantAccessDto>>(cached) ?? [];

            var tenants = await LoadTenantsFromMarketplaceAsync(email);
            await CacheTenantsAsync(cacheKey, tenants);
            return tenants;
        }

        /// <summary>
        /// Checks whether the given tenant name is in the user's allowed tenant list.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="tenantName">Tenant identifier from X-Tenant.</param>
        /// <returns>True when the tenant is in the cached or freshly loaded allowed list.</returns>
        public async Task<bool> IsTenantAllowedForUserAsync(string email, string tenantName)
        {
            var allowed = await FindAllowedTenantsByEmailAsync(email);
            return allowed.Any(t => t.Name.Equals(tenantName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Loads allowed tenants from the marketplace when the distributed cache has no entry.
        /// </summary>
        /// <param name="email">User email for the access check.</param>
        /// <returns>Allowed tenants, or an empty list when access is denied.</returns>
        /// <exception cref="InvalidOperationException">When KeyAccess is missing or the marketplace call fails.</exception>
        private async Task<IReadOnlyList<TenantAccessDto>> LoadTenantsFromMarketplaceAsync(string email)
        {
            var apiKey = _configuration["KeyAccess"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("KeyAccess is not configured.");

            try
            {
                var access = await _marketplace.CheckAccessByHub(apiKey, email);
                if (access == null || !access.HasAccess)
                    return [];

                return access.Tenants.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Marketplace CheckAccessByHub failed for {Email}", email);
                throw new InvalidOperationException("Unable to verify tenant access with marketplace.", ex);
            }
        }

        /// <summary>
        /// Persists the allowed tenant list in distributed cache using the configured TTL.
        /// </summary>
        /// <param name="cacheKey">Cache key for the normalized user email.</param>
        /// <param name="tenants">Tenants to serialize and store.</param>
        private async Task CacheTenantsAsync(string cacheKey, IReadOnlyList<TenantAccessDto> tenants)
        {
            var expirationMinutes = _configuration.GetValue("Cache:UserTenantAccessExpirationMinutes", 10);
            var json = JsonSerializer.Serialize(tenants);
            await _cache.SetStringAsync(
                cacheKey,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
                });
        }
    }
}
