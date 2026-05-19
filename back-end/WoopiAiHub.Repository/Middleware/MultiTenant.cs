using System.Text.Json;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Utils;
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
        /// <param name="tenantBindingValidator">Validates X-Tenant against JWT and marketplace access.</param>
        public async Task InvokeAsync(HttpContext context,
                                      IConfiguration configuration,
                                      ITenantCacheServices tenantCacheService,
                                      ITenantBindingValidator tenantBindingValidator)
        {
            if (!await tenantBindingValidator.TryValidateRequestBindingAsync(context))
            {
                await WriteForbiddenAsync(context);
                return;
            }

            var headerTenant = TryReadHeaderTenant(context);
            if (!string.IsNullOrEmpty(headerTenant))
                await TrySetTenantConnectionAsync(context, configuration, tenantCacheService, headerTenant);

            await _next(context);
        }

        private static string? TryReadHeaderTenant(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderNames.XTenant, out var values))
                return null;

            var tenantName = values.ToString();
            return string.IsNullOrWhiteSpace(tenantName) ? null : tenantName;
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
