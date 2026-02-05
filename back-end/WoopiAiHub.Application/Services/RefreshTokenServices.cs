using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class RefreshTokenServices : IRefreshTokenServices
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;

        public RefreshTokenServices(IDistributedCache cache, IConfiguration config)
        {
            _cache = cache;
            _config = config;
        }

        private DistributedCacheEntryOptions GetCacheOptions()
        {
            var days = _config.GetValue("JWT:RefreshTokenExpirationDays", 7);
            return new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(days)
            };
        }

        /// <summary>
        /// Saves the specified user email and associates it with the given refresh token in the cache.
        /// </summary>
        /// <remarks>This method stores the user email in the cache using a key derived from the provided
        /// refresh token. The cache options used for storage are determined by the <c>CacheOptions</c> field.</remarks>
        /// <param name="userEmail">The email address of the user to be saved. Cannot be null or empty.</param>
        /// <param name="refreshToken">The refresh token to associate with the user email. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveAsync(string userEmail, string refreshToken)
        {
            var key = GetKey(refreshToken);
            await _cache.SetStringAsync(key, userEmail, GetCacheOptions());
        }

        /// <summary>
        /// Retrieves the user identifier associated with the specified refresh token.
        /// </summary>
        /// <remarks>This method queries a cache to find the user identifier linked to the provided
        /// refresh token.  Ensure the refresh token is valid and properly formatted before calling this
        /// method.</remarks>
        /// <param name="refreshToken">The refresh token used to locate the associated user identifier.  This value cannot be null or empty.</param>
        /// <returns>A <see cref="string"/> representing the user identifier if the refresh token is found;  otherwise, <see
        /// langword="null"/>.</returns>
        public async Task<string?> FindUserByRefreshTokenAsync(string refreshToken)
        {
            var key = GetKey(refreshToken);
            return await _cache.GetStringAsync(key);
        }

        /// <summary>
        /// Revokes a refresh token by removing it from the cache.
        /// </summary>
        /// <param name="refreshToken">The refresh token to be revoked. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RevokeAsync(string refreshToken)
        {
            var key = GetKey(refreshToken);
            await _cache.RemoveAsync(key);
        }

        private static string GetKey(string refreshToken) => $"refresh_token:{refreshToken}";
    }
}
