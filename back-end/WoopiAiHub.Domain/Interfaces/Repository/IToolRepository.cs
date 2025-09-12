using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IToolRepository
    {
        Task<IEnumerable<ToolDto>> FindAllAsync();
        Task<ToolDto?> FindByIdAsync(int id);
        Task<Tool?> FindModelByIdAsync(int id);
        Task<bool> CreateUniqueAsync(Tool tool);
        Task<bool> UpdateAsync(Tool tool);
        Task<bool> DeleteAsync(List<int> ids);
        IQueryable<ToolDto> FindAllPaged();
    }
}
