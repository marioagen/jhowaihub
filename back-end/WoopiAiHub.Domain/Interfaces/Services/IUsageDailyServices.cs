using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageDailyServices
    {
        Task<bool> AddAsync(UsageDailyDto usageDailyDto);
        Task<bool> AddByValuesAsync(string usageTypeName, string email, int count, string modelEmbedding = "");
    }
}
