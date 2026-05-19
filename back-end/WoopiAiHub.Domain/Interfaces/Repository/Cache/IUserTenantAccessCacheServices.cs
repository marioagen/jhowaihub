using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository.Cache
{
    /// <summary>
    /// Resolves the list of tenants a user may access according to the marketplace, with distributed caching.
    /// </summary>
    public interface IUserTenantAccessCacheServices
    {
        /// <summary>
        /// Returns tenants the user is allowed to access for the Hub module.
        /// </summary>
        /// <param name="email">User email used for marketplace lookup.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Allowed tenants; empty when the user has no access.</returns>
        /// <exception cref="InvalidOperationException">When the marketplace call fails (fail-closed for runtime validation).</exception>
        Task<IReadOnlyList<TenantAccessDto>> FindAllowedTenantsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether the given tenant name is in the user's allowed tenant list.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="tenantName">Tenant identifier from X-Tenant.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<bool> IsTenantAllowedForUserAsync(
            string email,
            string tenantName,
            CancellationToken cancellationToken = default);
    }
}
