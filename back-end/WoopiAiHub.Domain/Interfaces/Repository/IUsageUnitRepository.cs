using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Automation;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUsageUnitRepository
    {
        Task<IEnumerable<UsageUnitDto>> FindAllAsync(DateFilterDto? dateFilterDto = null);
    }
}
