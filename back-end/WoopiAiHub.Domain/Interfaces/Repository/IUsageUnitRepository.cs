using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageUnitRepository
    {
        Task<IEnumerable<UsageUnit>> FindAllAsync();
    }
}
