using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class UsageMonthServices : IUsageMonthServices
    {
        private readonly IUsageMonthRepository _usageMonthRepository;

        public UsageMonthServices(IUsageMonthRepository usageMonthRepository)
        {
            _usageMonthRepository = usageMonthRepository;
        }

        /// <summary>
        /// Returns usage data filtered by usage type.
        /// </summary>
        /// <param name="usageType"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByUsageType(ColTypeUsage usageType)
        {
            return await _usageMonthRepository.FindDataByUsageType((int)usageType);
        }

        /// <summary>
        /// Finds usage data by model embedding ID.
        /// </summary>
        /// <param name="modelEmbeddingId"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(int modelEmbeddingId)
        {
            return await _usageMonthRepository.FindDataByModelEmbedding(modelEmbeddingId);
        }

        /// <summary>
        /// Finds used model embeddings.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings()
        {
            return await _usageMonthRepository.FindUsedModelEmbeddings();
        }
    }
}
