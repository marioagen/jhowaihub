using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageDailyRepository
    {
        Task<UsageDaily?> FindByIdAsync(int id);
        Task<bool> AddAsync(UsageDaily usageDaily);
        Task<bool> UpdateAsync(UsageDaily usageDaily);
        Task<bool> DeleteAsync(int id);
    }
}
