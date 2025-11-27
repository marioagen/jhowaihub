using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageMonthRepository
    {
        Task<UsageMonth?> FindByKeyAsync(int usageTypeId, int modelEmbeddingId, Guid userId, DateTime month, CancellationToken ct = default);
        Task UpsertAsync(UsageMonth entity, CancellationToken ct = default);
        Task<Dictionary<string, int>> GetTotalUsageByTenantsAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
    }
}
