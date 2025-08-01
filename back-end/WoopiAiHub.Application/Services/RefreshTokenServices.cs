using Microsoft.Extensions.Caching.Distributed;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class RefreshTokenServices : IRefreshTokenServices
    {
        private readonly IDistributedCache _cache;
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        };

        public RefreshTokenServices(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SaveAsync(string userEmail, string refreshToken)
        {
            var key = GetKey(refreshToken);
            await _cache.SetStringAsync(key, userEmail, CacheOptions);
        }

        public async Task<string?> FindUserByRefreshTokenAsync(string refreshToken)
        {
            var key = GetKey(refreshToken);
            return await _cache.GetStringAsync(key);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var key = GetKey(refreshToken);
            await _cache.RemoveAsync(key);
        }

        private static string GetKey(string refreshToken) => $"refresh_token:{refreshToken}";
    }
}
