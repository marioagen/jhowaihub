using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IToolDataRepository
    {
        Task<IEnumerable<ToolDataDto>> FindAllAsync();
    }
}
