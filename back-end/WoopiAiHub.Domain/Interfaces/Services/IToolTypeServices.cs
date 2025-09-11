using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IToolTypeServices
    {
        Task<IEnumerable<ToolTypeDto>> FindAllAsync();
    }
}
