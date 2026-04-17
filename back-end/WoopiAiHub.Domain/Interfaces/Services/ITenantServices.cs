using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services { 
    public interface ITenantServices
    {
        Task InitializeTenant(string tenant);
        Task<DashboardTenantInfo> FindPlanByName(string tenant);
        void ProcessSubscription(TenantSubscriptionDto tenantSubscriptionDto);
    }
}
