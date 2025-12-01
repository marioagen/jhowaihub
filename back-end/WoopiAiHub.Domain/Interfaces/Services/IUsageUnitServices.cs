using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUsageUnitServices
    {
        Task<IEnumerable<UsageUnitDto>> FindAllAsync();
    }
}
