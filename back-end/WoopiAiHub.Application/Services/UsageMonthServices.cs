using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils;

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
        /// <param name="usageMonthFilterDto"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByUsageType(UsageTypeFilterDto usageMonthFilterDto)
        {
            var startDate = DateHelper.ParseDate(usageMonthFilterDto.Start);
            var endDate = DateHelper.ParseDate(usageMonthFilterDto.End);

            return await _usageMonthRepository.FindDataByUsageType(usageMonthFilterDto.UsageType, startDate, endDate, usageMonthFilterDto.WorkflowIds);
        }

        /// <summary>
        /// Finds usage data by model embedding ID.
        /// </summary>
        /// <param name="modelEmbeddingFilterDto"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(ModelEmbeddingFilterDto modelEmbeddingFilterDto)
        {
            var startDate = DateHelper.ParseDate(modelEmbeddingFilterDto.Start);
            var endDate = DateHelper.ParseDate(modelEmbeddingFilterDto.End);
            return await _usageMonthRepository.FindDataByModelEmbedding(modelEmbeddingFilterDto.Id, startDate, endDate, modelEmbeddingFilterDto.WorkflowIds);
        }

        /// <summary>
        /// Finds used model embeddings.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ModelEmbeddingDto>> FindUsedModelEmbeddings()
        {
            return await _usageMonthRepository.FindUsedModelEmbeddings();
        }

        /// <summary>
        /// Finds total usage cost.
        /// </summary>
        /// <param name="dateFilterDto"></param>
        /// <returns></returns>
        public async Task<decimal> FindTotalUsageCostAsync(DateFilterDto dateFilterDto)
        {
            var startDate = DateHelper.ParseDate(dateFilterDto.Start);
            var endDate = DateHelper.ParseDate(dateFilterDto.End);
            return await _usageMonthRepository.FindTotalUsageCostAsync(startDate, endDate);
        }
    }
}
