using System.Security.Claims;
using System.Text.Json;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WoopiAiHub.Repository.Middleware
{
    public class MultiTenant
    {
        private const string TenantMismatchMessage = "Tenant mismatch or missing.";
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes the middleware with the next delegate in the pipeline.
        /// </summary>
        /// <param name="next">The next middleware to invoke when tenant validation succeeds.</param>
        public MultiTenant(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Validates tenant binding for authenticated requests, sets TenantConnection when allowed,
        /// and forwards the request to the next middleware.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="configuration">Application configuration used to build the tenant connection string.</param>
        /// <param name="tenantCacheService">Service that resolves tenant metadata from the cache.</param>
        public async Task InvokeAsync(HttpContext context,
                                      IConfiguration configuration,
                                      ITenantCacheServices tenantCacheService)
        {
            var headerTenant = TryReadHeaderTenant(context);
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
            var claimTenant = TryReadClaimTenant(context.User);

            if (!TryValidateTenantBinding(isAuthenticated, headerTenant, claimTenant))
            {
                await WriteForbiddenAsync(context);
                return;
            }

            if (!string.IsNullOrEmpty(headerTenant))
            {
                await TrySetTenantConnectionAsync(context, configuration, tenantCacheService, headerTenant);
            }
            await _next(context);
        }

        /// <summary>
        /// Reads the tenant identifier from the X-Tenant request header.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>The header value when present and non-empty; otherwise, null.</returns>
        private static string? TryReadHeaderTenant(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderNames.XTenant, out var values))
                return null;

            var tenantName = values.ToString();
            return string.IsNullOrWhiteSpace(tenantName) ? null : tenantName;
        }

        /// <summary>
        /// Reads the tenant claim from the authenticated user's JWT.
        /// </summary>
        /// <param name="user">The claims principal for the current request.</param>
        /// <returns>The tenant claim value when present and non-empty; otherwise, null.</returns>
        private static string? TryReadClaimTenant(ClaimsPrincipal user)
        {
            var claimTenant = user.FindFirst(JwtClaimNames.Tenant)?.Value;
            return string.IsNullOrWhiteSpace(claimTenant) ? null : claimTenant;
        }

        /// <summary>
        /// Determines whether the X-Tenant header is consistent with the JWT tenant claim for authenticated requests.
        /// </summary>
        /// <param name="isAuthenticated">Whether the current user is authenticated.</param>
        /// <param name="headerTenant">Tenant from the X-Tenant header, if any.</param>
        /// <param name="claimTenant">Tenant from the JWT claim, if any.</param>
        /// <returns>
        /// True when validation passes (unauthenticated requests always pass;
        /// authenticated requests require matching header and claim when either is present).
        /// </returns>
        private static bool TryValidateTenantBinding(bool isAuthenticated, string? headerTenant, string? claimTenant)
        {
            if (!isAuthenticated)
                return true;

            if (!string.IsNullOrEmpty(claimTenant) && string.IsNullOrEmpty(headerTenant))
                return false;

            if (!string.IsNullOrEmpty(headerTenant) && string.IsNullOrEmpty(claimTenant))
                return false;

            if (!string.IsNullOrEmpty(headerTenant) && !string.IsNullOrEmpty(claimTenant))
                return headerTenant.Equals(claimTenant, StringComparison.OrdinalIgnoreCase);

            return true;
        }

        /// <summary>
        /// Writes a 403 Forbidden JSON response when tenant header and JWT claim do not match.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        private static async Task WriteForbiddenAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = TenantMismatchMessage }));
        }

        /// <summary>
        /// Resolves the tenant database and stores its connection string in HttpContext.Items.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="configuration">Application configuration used to build the connection string.</param>
        /// <param name="tenantCacheService">Service that resolves tenant metadata from the cache.</param>
        /// <param name="tenantName">The tenant identifier to resolve.</param>
        private static async Task TrySetTenantConnectionAsync(HttpContext context,
                                                                IConfiguration configuration,
                                                                ITenantCacheServices tenantCacheService,
                                                                string tenantName)
        {
            var tenant = await tenantCacheService.FindTenantAsync(tenantName);

            if (tenant == null || string.IsNullOrEmpty(tenant.DatabaseName))
                return;

            var template = configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenant.DatabaseName);
            context.Items["TenantConnection"] = connectionString;
        }
    }

    /// <summary>
    /// Registers the multi-tenant middleware in the HTTP request pipeline.
    /// </summary>
    public static class MultiTenantExtension
    {
        /// <summary>
        /// Adds middleware that validates tenant binding and sets the per-request tenant database connection.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The same application builder for chaining.</returns>
        public static IApplicationBuilder UseMultiTenantExtension(this IApplicationBuilder app)
        {
            app.UseMiddleware<MultiTenant>();
            return app;
        }
    }
}
