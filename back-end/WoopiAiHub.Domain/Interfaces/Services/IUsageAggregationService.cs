namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageAggregationService
    {
        Task ProcessUnprocessedUsageAsync(CancellationToken ct = default);
    }
}
