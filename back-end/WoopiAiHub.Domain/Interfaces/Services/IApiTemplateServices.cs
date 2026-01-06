using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IApiTemplateServices
    {
        Task<ApiTemplateDto> FindById(Guid id);
        Task<bool> DeleteById(Guid id);
        Task<ICollection<ApiTemplateDto>> FindAll(ApiTemplateFilterDto filter);
        PaginatedListDto<ApiTemplateDto> FindAllPaged(ApiTemplatePagedFilterDto filter);
        Task<bool> CreateAsync(ApiTemplateCreateDto templateCreateDto);
        Task<bool> UpdateAsync(ApiTemplateUpdateDto templateUpdateDto);
    }
}
