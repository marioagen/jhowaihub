using System.Globalization;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
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
        /// <param name="usageMonthFilterDto"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByUsageType(UsageTypeFilterDto usageMonthFilterDto)
        {
            var startDate = FindDate(usageMonthFilterDto.Start);
            var endDate = FindDate(usageMonthFilterDto.End);

            return await _usageMonthRepository.FindDataByUsageType(usageMonthFilterDto.UsageType, startDate, endDate);
        }

        /// <summary>
        /// Finds usage data by model embedding ID.
        /// </summary>
        /// <param name="modelEmbeddingFilterDto"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(ModelEmbeddingFilterDto modelEmbeddingFilterDto)
        {
            var startDate = FindDate(modelEmbeddingFilterDto.Start);
            var endDate = FindDate(modelEmbeddingFilterDto.End);
            return await _usageMonthRepository.FindDataByModelEmbedding(modelEmbeddingFilterDto.Id, startDate, endDate);
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
        /// Converts a string date to DateTime?.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static DateTime? FindDate(string? date)
        {
            DateTime? convertedDate = null;
            if (string.IsNullOrEmpty(date) is false)
            {
                convertedDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return convertedDate;
        }

        /// <summary>
        /// Finds total usage cost.
        /// </summary>
        /// <param name="dateFilterDto"></param>
        /// <returns></returns>
        public async Task<decimal> FindTotalUsageCostAsync(DateFilterDto dateFilterDto)
        {
            var startDate = FindDate(dateFilterDto.Start);
            var endDate = FindDate(dateFilterDto.End);
            return await _usageMonthRepository.FindTotalUsageCostAsync(startDate, endDate);
        }
    }
}
