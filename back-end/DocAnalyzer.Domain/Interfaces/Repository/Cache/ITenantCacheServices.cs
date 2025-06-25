using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.Enum;

namespace DocAnalyzer.Domain.Interfaces.Repository.Cache
{
    public interface ITenantCacheServices
    {
        Task<TenantInfoDto?> FindTenantAsync(string tenantName,
                                             ColTypeModule module);
    }
}
