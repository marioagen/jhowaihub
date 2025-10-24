using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IToolTypeRepository
    {
        Task<IEnumerable<ToolTypeDto>> FindAllAsync();
        Task<ToolTypeDto?> FindByAsync(int id);
        Task<ToolType?> FindModelByIdAsync(int id);
    }
}
