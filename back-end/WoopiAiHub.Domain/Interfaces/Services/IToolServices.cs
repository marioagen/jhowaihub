using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IToolServices
    {
        Task<IEnumerable<ToolDto>> FindAllAsync();
        Task<ToolDto?> FindByIdAsync(int id);
        Task<bool> CreateAsync(ToolCreateDto toolCreateDto);
        Task<bool> UpdateAsync(ToolUpdateDto toolUpdateDto);
        Task<bool> DeleteAsync(List<int> ids);
        PagedResponseDto<ToolDto> FindAllPaged(PagedDataDto pagedDataDto);
    }
}
