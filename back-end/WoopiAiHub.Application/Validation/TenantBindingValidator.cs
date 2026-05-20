using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Validation
{
    public class TenantBindingValidator : ITenantBindingValidator
    {
        private readonly IUserTenantAccessCacheServices _userTenantAccessCache;
        private readonly ILogger<TenantBindingValidator> _logger;

        public TenantBindingValidator(
            IUserTenantAccessCacheServices userTenantAccessCache,
            ILogger<TenantBindingValidator> logger)
        {
            _userTenantAccessCache = userTenantAccessCache;
            _logger = logger;
        }

        /// <summary>
        /// Validates tenant binding for the current HTTP request (flexible mode: allowed list OR header equals claim).
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when the request may proceed; false when a 403 should be returned.</returns>
        public async Task<bool> TryValidateRequestBindingAsync(
            HttpContext context,
            CancellationToken cancellationToken = default)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return true;

            var headerTenant = TryReadHeaderTenant(context);
            var claimTenant = TryReadClaimTenant(context.User);

            if (!string.IsNullOrEmpty(claimTenant) && string.IsNullOrEmpty(headerTenant))
            {
                if (IsSignalRHubRequest(context))
                    return true;

                return false;
            }

            if (string.IsNullOrEmpty(headerTenant))
                return true;

            var email = ResolveUserEmail(context.User);
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!string.IsNullOrEmpty(claimTenant)
                && headerTenant.Equals(claimTenant, StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                return await _userTenantAccessCache.IsTenantAllowedForUserAsync(email, headerTenant);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Tenant binding validation failed for {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Resolves a tenant from the marketplace list and ensures its database is ready.
        /// </summary>
        /// <param name="tenant">Requested tenant name.</param>
        /// <param name="tenants">Tenants returned by marketplace access check.</param>
        /// <returns>The validated tenant name.</returns>
        public string FindAndValidateTenant(string tenant, ICollection<TenantAccessDto> tenants)
        {
            var tenantFound = tenants.FirstOrDefault(t =>
                t.Name.Equals(tenant, StringComparison.OrdinalIgnoreCase));

            if (tenantFound == null)
            {
                throw new AppException(
                    null,
                    "Tenant not found",
                    Login.TenantNotFound);
            }

            if (!tenantFound.IsDatabaseCreated)
            {
                throw new AppException(
                    ErrorCode.BusinessWarningOutput,
                    "Tenant database is not ready or cannot be accessed.",
                    Login.TenantDatabaseNotReady);
            }

            return tenantFound.Name;
        }

        /// <summary>
        /// Reads the tenant name from the X-Tenant request header when present and non-empty.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// <returns>Header value, or null when the header is missing or blank.</returns>
        private static string? TryReadHeaderTenant(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderNames.XTenant, out var values))
                return null;

            var tenantName = values.ToString();
            return string.IsNullOrWhiteSpace(tenantName) ? null : tenantName;
        }

        /// <summary>
        /// Reads the tenant name from the JWT tenant claim when present and non-empty.
        /// </summary>
        /// <param name="user">Authenticated principal.</param>
        /// <returns>Claim value, or null when the claim is missing or blank.</returns>
        private static string? TryReadClaimTenant(ClaimsPrincipal user)
        {
            var claimTenant = user.FindFirst(JwtClaimNames.Tenant)?.Value;
            return string.IsNullOrWhiteSpace(claimTenant) ? null : claimTenant;
        }

        /// <summary>
        /// Resolves the user email from standard email or JWT subject claims.
        /// </summary>
        /// <param name="user">Authenticated principal.</param>
        /// <returns>Email or subject identifier, or null when neither claim is present.</returns>
        private static string? ResolveUserEmail(ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        private static bool IsSignalRHubRequest(HttpContext context) =>
            context.Request.Path.StartsWithSegments(HubRoutePaths.NotificationsHub);
    }
}
