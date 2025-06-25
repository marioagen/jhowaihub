using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.Enum;
using DocAnalyzer.Domain.Interfaces.Refit;
using DocAnalyzer.Domain.Interfaces.Repository.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DocAnalyzer.Repository.Cache
{
    public class TenantCacheService : ITenantCacheServices
    {
        private readonly IDistributedCache _cache;
        private readonly IMarketPlaceApi _marketplace;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(60);

        public TenantCacheService(IDistributedCache cache,
                                  IMarketPlaceApi marketplace,
                                  IConfiguration configuration)
        {
            _cache = cache;
            _marketplace = marketplace;
            _configuration = configuration;
        }

        /// <summary>
        /// Search for tenant data. Check if it is cached or search the Marketplace
        /// </summary>
        /// <param name="tenantName"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public async Task<TenantInfoDto?> FindTenantAsync(string tenantName,
                                                          ColTypeModule module)
        {
            var cacheKey = $"tenant:{tenantName}:{module}";
            var cached = await _cache.GetStringAsync(cacheKey);
            var apiKey = _configuration["KeyAccess"];

            if (!string.IsNullOrWhiteSpace(cached))
                return JsonSerializer.Deserialize<TenantInfoDto>(cached);

            if (string.IsNullOrEmpty(tenantName))
            {
                return new TenantInfoDto();
            }

            var tenant = await _marketplace.FindTenantByNameAndModule(apiKey,
                                                                      tenantName,
                                                                      module);
            if (tenant != null)
            {
                var json = JsonSerializer.Serialize(tenant);
                await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _expiration
                });
            }

            return tenant;
        }
    }
}
