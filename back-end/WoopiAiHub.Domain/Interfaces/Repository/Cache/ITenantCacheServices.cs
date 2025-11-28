using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Repository.Cache
{
    public interface ITenantCacheServices
    {
        Task<TenantInfoDto?> FindTenantAsync(string tenantName);
    }
}
