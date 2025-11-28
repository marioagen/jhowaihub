using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageDailyRepository
    {
        Task<List<UsageDaily>> FindUnprocessedAsync();
        Task<List<UsageDaily>> FindOldRecordsAsync(DateTime cutoffDate);
        Task MarkAsProcessedAsync(IEnumerable<int> ids);
        Task BulkDeleteAsync(IEnumerable<int> ids);
    }
}
