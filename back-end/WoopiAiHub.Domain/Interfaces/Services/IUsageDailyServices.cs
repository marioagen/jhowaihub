using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageDailyServices
    {
        Task<bool> AddAsync(UsageDailyDto usageDailyDto);
        Task<bool> AddRangeAsync(List<UsageDailyDto> usageDailyDtos);
        Task<bool> AddByValuesAsync(string usageTypeName, string email, int count, string modelEmbedding = "", int? workflowId = null);
        Task<bool> AddByRangeValuesAsync(string usageTypeName, string email, List<QueryUsageDto> usages, int? workflowId = null);
    }
}
