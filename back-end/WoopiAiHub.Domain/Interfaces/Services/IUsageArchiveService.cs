namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageArchiveService
    {
        Task ArchiveOldUsageAsync(CancellationToken ct = default);
    }
}
