using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageMonthRepository
    {
        Task<ICollection<DashboardUsageDto>> FindDataByUsageType(int usageTypeId);
        Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId);
        Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings();
    }
}
