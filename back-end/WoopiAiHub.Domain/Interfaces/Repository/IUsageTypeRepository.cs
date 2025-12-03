using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageTypeRepository
    {
        Task<UsageType?> FindByNameAsync(string name);
        Task<UsageType?> FindByIdAsync(int id);
        Task<bool> AddAsync(UsageType usageType);
        Task<bool> UpdateAsync(UsageType usageType);
        Task<bool> DeleteAsync(int id);
    }
}
