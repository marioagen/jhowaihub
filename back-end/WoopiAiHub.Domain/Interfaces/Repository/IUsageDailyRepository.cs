using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageDailyRepository
    {
        Task<List<UsageDaily>> FindUnprocessedAsync();
        Task<List<UsageDaily>> GetOldRecordsAsync(DateTime cutoffDate, int batchSize, CancellationToken ct = default);
        Task MarkAsProcessedAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task BulkDeleteAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
