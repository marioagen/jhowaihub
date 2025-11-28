using System.Globalization;
using WoopiAiHub.Domain.DTOs.Request;
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
        public async Task<ICollection<DashboardUsageDto>> FindDataByUsageType(UsageTypeFilterDto usageMonthFilterDto)
        {
            var startDate = GetDate(usageMonthFilterDto.Start);
            var endDate = GetDate(usageMonthFilterDto.End);

            return await _usageMonthRepository.FindDataByUsageType(usageMonthFilterDto.Id, startDate, endDate);
        }

        /// <summary>
        /// Finds usage data by model embedding ID.
        /// </summary>
        /// <param name="modelEmbeddingId"></param>
        /// <returns></returns>
        public async Task<ICollection<DashboardUsageDto>> FindDataByModelEmbedding(ModelEmbeddingFilterDto modelEmbeddingFilterDto)
        {
            var startDate = GetDate(modelEmbeddingFilterDto.Start);
            var endDate = GetDate(modelEmbeddingFilterDto.End);
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
        private static DateTime? GetDate(string? date)
        {
            DateTime? convertedDate = null;
            if (string.IsNullOrEmpty(date) is false)
            {
                convertedDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return convertedDate;
        }
    }
}
