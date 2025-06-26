using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WoopiAiHub.Repository.Middleware
{
    public class MultiTenant
    {
        private readonly RequestDelegate _next;

        public MultiTenant(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context,
                                      IConfiguration configuration,
                                      ITenantCacheServices tenantCacheService)
        {
            string? tenantName = null;

            if (context.Request.Headers.TryGetValue(HeaderNames.XTenant, out var values))
            {
                tenantName = values.ToString();

                var tenant = await tenantCacheService.FindTenantAsync(tenantName,
                                                                      ColTypeModule.DocAnalyzer);

                if (tenant != null)
                {
                    var template = configuration.GetConnectionString("TemplateConnection");
                    var connectionString = template?.Replace("___NEWDB___", tenant.DatabaseName);
                    context.Items["TenantConnection"] = connectionString;
                }
            }

            await _next(context);
        }
    }

    public static class MultiTenantExtension
    {
        public static IApplicationBuilder UseMultiTenantExtension(this IApplicationBuilder app)
        {
            app.UseMiddleware<MultiTenant>();
            return app;
        }
    }
}