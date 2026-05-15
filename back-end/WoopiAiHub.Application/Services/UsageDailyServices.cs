using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
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
                usageDailyDto.ModelEmbeddingId,
                usageDailyDto.WorkflowId,
                usageDailyDto.Origin
            );

            return await _usageDailyRepository.AddAsync(usageDaily);
        }

        /// <summary>
        /// Add a range of new usage daily records
        /// </summary>
        /// <param name="usageDailyDtos"></param>
        /// <returns></returns>
        public async Task<bool> AddRangeAsync(List<UsageDailyDto> usageDailyDtos)
        {
            var usageDailies = usageDailyDtos.Select(usageDailyDto => new UsageDaily(
                0,
                DateTime.UtcNow,
                usageDailyDto.UserId,
                usageDailyDto.UsageTypeId,
                usageDailyDto.UsageCount,
                usageDailyDto.Processed,
                usageDailyDto.ModelEmbeddingId,
                usageDailyDto.WorkflowId,
                usageDailyDto.Origin
            )).ToList();

            return await _usageDailyRepository.AddRangeAsync(usageDailies);
        }

        /// <summary>
        /// Add a new usage daily record by values
        /// </summary>
        /// <param name="usageTypeName"></param>
        /// <param name="email"></param>
        /// <param name="count"></param>
        /// <param name="modelEmbedding"></param>
        /// <param name="workflowId"></param>
        /// <param name="origin">Origin of the usage record. Defaults to <see cref="UsageDailyOrigin.WoopiAi"/>.</param>
        /// <returns></returns>
        public async Task<bool> AddByValuesAsync(string usageTypeName, string email, int count, string modelEmbedding = "", int? workflowId = null, UsageDailyOrigin origin = UsageDailyOrigin.WoopiAi)
        {
            var usageType = await _usageTypeServices.FindByNameAsync(usageTypeName);
            if (usageType == null)
                return false;

            var userId = _userServices.FindIdByEmail(email);
            if (userId == Guid.Empty)
                return false;

            int? modelEmbeddingId = null;
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
                modelEmbeddingId,
                false,
                workflowId,
                origin
            );

            return await AddAsync(usageDailyDto);
        }

        /// <summary>
        /// Add a new usage daily record by values with multiple usages
        /// </summary>
        /// <param name="usageTypeName"></param>
        /// <param name="email"></param>
        /// <param name="usages"></param>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public async Task<bool> AddByRangeValuesAsync(string usageTypeName, string email, List<QueryUsageDto> usages, int? workflowId = null)
        {
            var usageType = await _usageTypeServices.FindByNameAsync(usageTypeName);
            if (usageType is null)
                return false;

            var userId = _userServices.FindIdByEmail(email);
            if (userId == Guid.Empty)
                return false;

            var usagesGroup = usages.GroupBy(u => u.Model)
                .Select(u => new
                {
                    Model = u.Key,
                    TotalUsage = u.Sum(usage => usage.Total_usage ?? 0)
                })
                .ToList();

            var modelUsages = new List<(string Model, int ModelId, int TotalUsage)>();
            var allModels = usagesGroup.Select(g => g.Model).ToList();
            var modelEmbeddings = await _modelEmbeddingRepository.FindAllByNamesListAsync(allModels);

            foreach (var g in usagesGroup)
            {
                var modelEmbeddingEntity = modelEmbeddings.FirstOrDefault(m => m.Name == g.Model);
                if (modelEmbeddingEntity is null)
                    continue;

                modelUsages.Add((
                    g.Model,
                    modelEmbeddingEntity.Id,
                    g.TotalUsage
                ));
            }

            var usageDailyDtos = modelUsages
                .Select(m => new UsageDailyDto(
                    usageType.Id,
                    m.TotalUsage,
                    userId,
                    m.ModelId,
                    false,
                    workflowId
                ))
                .ToList();

            return await AddRangeAsync(usageDailyDtos);

        }
    }
}
