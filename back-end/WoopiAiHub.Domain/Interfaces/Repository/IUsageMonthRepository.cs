using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageMonthRepository
    {
        Task<UsageMonth?> FindByKeyAsync(int usageTypeId, int? modelEmbeddingId, Guid userId, DateTime month, int? workflowId);
        Task UpsertAsync(UsageMonth entity);
        Task<int> FindTotalUsageAsync(DateTime periodStart, DateTime periodEnd);
        Task<ICollection<DashboardUsageDto>> FindDataByUsageType(string usageType, DateTime? start, DateTime? end, List<int>? workflowIds);
        Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId, DateTime? start, DateTime? end, List<int>? workflowIds);
        Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings();
        Task<decimal> FindTotalUsageCostAsync(DateTime? periodStart, DateTime? periodEnd, List<int>? workflowIds);
    }
}
