using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageLogRepository
    {
        Task BulkInsertAsync(IEnumerable<UsageLog> logs, CancellationToken ct = default);
        Task<bool> ExistsAsync(int originalId, DateTime created, CancellationToken ct = default);
    }
}
