using Microsoft.AspNetCore.Http;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    /// <summary>
    /// Validates X-Tenant header binding against JWT claims and marketplace-allowed tenants.
    /// </summary>
    public interface ITenantBindingValidator
    {
        /// <summary>
        /// Validates tenant binding for the current HTTP request (flexible mode: allowed list OR header equals claim).
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when the request may proceed; false when a 403 should be returned.</returns>
        Task<bool> TryValidateRequestBindingAsync(HttpContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a tenant from the marketplace list and ensures its database is ready.
        /// </summary>
        /// <param name="tenant">Requested tenant name.</param>
        /// <param name="tenants">Tenants returned by marketplace access check.</param>
        /// <returns>The validated tenant name.</returns>
        string FindAndValidateTenant(string tenant, ICollection<TenantAccessDto> tenants);
    }
}
