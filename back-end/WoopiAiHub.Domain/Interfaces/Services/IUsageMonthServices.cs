using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageMonthServices
    {
        Task<ICollection<DashboardUsageDto>> FindDataByUsageType(ColTypeUsage usageType);
        Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId);
        Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings();
    }
}
