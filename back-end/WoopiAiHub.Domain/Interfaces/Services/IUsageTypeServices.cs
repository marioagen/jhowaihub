using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageTypeServices
    {
        Task<UsageType?> FindByNameAsync(string name);
    }
}
