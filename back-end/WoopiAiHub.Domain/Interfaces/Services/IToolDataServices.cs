using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IToolDataServices
    {
        Task<IEnumerable<ToolDataDto>> FindAllAsync();
    }
}
