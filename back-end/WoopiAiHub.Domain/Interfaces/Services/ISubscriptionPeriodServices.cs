using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ISubscriptionPeriodServices
    {
        Task<SubscriptionPeriod> CreateAsync(DateTime periodStart, DateTime periodEnd, bool isProcessed);
        Task<SubscriptionPeriod?> GetLastUnprocessedAsync();
        Task UpdateToProcessedAsync(int id);
    }
}
