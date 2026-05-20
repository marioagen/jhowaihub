using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository.Cache
{
    public interface IUserTenantAccessCacheServices
    {
        Task<IReadOnlyList<TenantAccessDto>> FindAllowedTenantsByEmailAsync(string email);
        Task<bool> IsTenantAllowedForUserAsync(string email, string tenantName);
    }
}
