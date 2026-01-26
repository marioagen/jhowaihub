using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ISubscriptionPeriodRepository
    {
        Task<SubscriptionPeriod> CreateAsync(SubscriptionPeriod subscriptionPeriod);
        Task<SubscriptionPeriod?> FindLastUnprocessedAsync();
        Task<SubscriptionPeriod?> FindByIdAsync(int id);
        Task UpdateAsync(SubscriptionPeriod subscriptionPeriod);
    }
}
