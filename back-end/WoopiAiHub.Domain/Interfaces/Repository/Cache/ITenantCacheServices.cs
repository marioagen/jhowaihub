using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Repository.Cache
{
    public interface ITenantCacheServices
    {
        Task<TenantInfoDto?> FindTenantAsync(string tenantName,
                                             ColTypeModule module);

        Task<List<TenantListDto>> FindAllTenantsAsync(ColTypeModule module);
    }
}
