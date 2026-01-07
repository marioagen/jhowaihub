using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageMonthServices
    {
        Task<ICollection<DashboardUsageDto>> FindDataByUsageType(UsageTypeFilterDto usageMonthFilterDto);
        Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(ModelEmbeddingFilterDto modelEmbeddingFilterDto);
        Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings();
        Task<decimal> FindTotalUsageCostAsync(DateFilterDto dateFilterDto);
    }
}
