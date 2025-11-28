using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageMonthRepository
    {
        Task<UsageMonth?> FindByKeyAsync(int usageTypeId, int modelEmbeddingId, Guid userId, DateTime month);
        Task UpsertAsync(UsageMonth entity);
        Task<int> FindTotalUsageAsync(DateTime periodStart, DateTime periodEnd);
        Task<ICollection<DashboardUsageDto>> FindDataByUsageType(int usageTypeId, DateTime? start, DateTime? end);
        Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId, DateTime? start, DateTime? end);
        Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings();
    }
}
