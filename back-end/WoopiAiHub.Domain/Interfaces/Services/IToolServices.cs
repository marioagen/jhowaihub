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
        bool Delete(List<int> ids);
        PagedResponseDto<ToolDto> FindAllPaged(ToolPagedDataDto toolPagedDataDto);
        Task<bool> ValidateConnector(ToolConnectorDto toolConnectorDto);
    }
}
