using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ITenantServices
    {
        Task<IEnumerable<string>> FindAllByUserEmail(string email);

        Task<string> InitializeTenant(string tenant);

        void ProcessSubscription(TenantSubscriptionDto tenantActivationDto);
    }
}