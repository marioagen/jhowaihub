using Microsoft.AspNetCore.Http;

namespace WoopiAiHub.Infrastructure.Multitenancy
{
    public interface ITenantContextService
    {
        Task<bool> TrySetTenantConnectionAsync(HttpContext context, string tenantName);
        Task InitializeTenantAsync(string tenantName);
    }
}
