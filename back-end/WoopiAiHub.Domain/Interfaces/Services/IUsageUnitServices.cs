using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageUnitServices
    {
        Task<IEnumerable<UsageUnit>> FindAllAsync();
    }
}
