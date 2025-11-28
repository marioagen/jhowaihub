using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageMonthRepository
    {
        Task<ICollection<DashboardUsageDto>> FindDataByUsageType(int usageTypeId, DateTime? start, DateTime? end);
        Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId, DateTime? start, DateTime? end);
        Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings();
    }
}
