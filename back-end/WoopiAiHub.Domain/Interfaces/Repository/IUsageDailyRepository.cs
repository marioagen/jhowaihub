using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageDailyRepository
    {
        Task<List<UsageDaily>> FindUnprocessedAsync();
        Task<List<UsageDaily>> FindOldRecordsAsync(DateTime cutoffDate);
        Task MarkAsProcessedAsync(IEnumerable<int> ids);
        Task BulkDeleteAsync(IEnumerable<int> ids);
        Task<UsageDaily?> FindByIdAsync(int id);
        Task<bool> AddAsync(UsageDaily usageDaily);
        Task<bool> UpdateAsync(UsageDaily usageDaily);
        Task<bool> DeleteAsync(int id);
        Task<bool> AddRangeAsync(List<UsageDaily> usageDailies);
    }
}
