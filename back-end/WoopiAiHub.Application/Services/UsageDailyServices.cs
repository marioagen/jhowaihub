using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageDailyServices : IUsageDailyServices
    {
        private readonly IUsageDailyRepository _usageDailyRepository;
        private readonly IUsageTypeServices _usageTypeServices;
        private readonly IUserServices _userServices;
        private readonly IModelEmbeddingRepository _modelEmbeddingRepository;

        public UsageDailyServices(IUsageDailyRepository usageDailyRepository,
                                  IUsageTypeServices usageTypeServices,
                                  IUserServices userServices,
                                  IModelEmbeddingRepository modelEmbeddingRepository)
        {
            _usageDailyRepository = usageDailyRepository;
            _usageTypeServices = usageTypeServices;
            _userServices = userServices;
            _modelEmbeddingRepository = modelEmbeddingRepository;
        }

        /// <summary>
        /// Add a new usage daily record
        /// </summary>
        /// <param name="usageDailyDto"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(UsageDailyDto usageDailyDto)
        {
            var usageDaily = new UsageDaily(
                0,
                DateTime.UtcNow,
                usageDailyDto.UserId,
                usageDailyDto.UsageTypeId,
                usageDailyDto.UsageCount,
                usageDailyDto.Processed,
                usageDailyDto.ModelEmbeddingId
            );

            return await _usageDailyRepository.AddAsync(usageDaily);
        }

        /// <summary>
        /// Add a new usage daily record by values
        /// </summary>
        /// <param name="usageTypeName"></param>
        /// <param name="email"></param>
        /// <param name="count"></param>
        /// <param name="modelEmbedding"></param>
        /// <returns></returns>
        public async Task<bool> AddByValuesAsync(string usageTypeName, string email, int count, string modelEmbedding = "")
        {
            var usageType = await _usageTypeServices.FindByNameAsync(usageTypeName);
            if (usageType == null) return false;

            var userId = _userServices.FindIdByEmail(email);
            if (userId == Guid.Empty) return false;

            var modelEmbeddingId = 0;
            if (!string.IsNullOrEmpty(modelEmbedding))
            {
                var modelEmbeddingEntity = await _modelEmbeddingRepository.FindByNameAsync(modelEmbedding);
                if (modelEmbeddingEntity != null)
                {
                    modelEmbeddingId = modelEmbeddingEntity.Id;
                }
            }

            var usageDailyDto = new UsageDailyDto(
                usageType.Id,
                count,
                userId,
                modelEmbeddingId
            );

            return await AddAsync(usageDailyDto);
        }
    }
}
