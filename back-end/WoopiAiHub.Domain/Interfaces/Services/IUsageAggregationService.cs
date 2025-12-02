namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageAggregationService
    {
        Task ProcessUnprocessedUsageAsync();
        Task ProcessUnprocessedUsageByTenantAsync(string tenantName);
    }
}
