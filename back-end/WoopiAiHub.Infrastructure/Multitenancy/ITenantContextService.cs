using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WoopiAiHub.Infrastructure.Multitenancy
{
    public interface ITenantContextService
    {
        Task<bool> TrySetTenantConnectionAsync(HttpContext context, string tenantIdentifier);
        Task InitializeTenantAsync(string tenantIdentifier);
        Task<string> FindConnectionStringAndHttpAcessorAsync(string tenantName, IServiceScope scope);
    }
}
