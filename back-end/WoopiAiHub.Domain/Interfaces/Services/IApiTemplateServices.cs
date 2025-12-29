using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IApiTemplateServices
    {
        Task<ApiTemplateDto> FindById(Guid id);
        Task<bool> DeleteById(Guid id);
        Task<ICollection<ApiTemplateDto>> FindAll(ApiTemplateFilterDto templateDto);
        PaginatedListDto<ApiTemplateDto> FindAllPaged(ApiTemplatePagedFilterDto templatePagedDto);
        Task<bool> CreateAsync(ApiTemplateCreateDto templateCreateDto);
        Task<bool> UpdateAsync(ApiTemplateUpdateDto templateUpdateDto);
    }
}
