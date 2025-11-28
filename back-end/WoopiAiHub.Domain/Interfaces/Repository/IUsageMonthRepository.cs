using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageMonthRepository
    {
        Task<UsageMonth?> FindByKeyAsync(int usageTypeId, int modelEmbeddingId, Guid userId, DateTime month);
        Task UpsertAsync(UsageMonth entity);
        Task<int> FindTotalUsageAsync(DateTime periodStart, DateTime periodEnd);
    }
}
