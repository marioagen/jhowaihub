using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IApiTemplateRepository
    {
        Task<ApiTemplateDto?> FindById(int id);
        Task<ApiTemplate?> FindByIdReturnModel(int id);
        Task<ICollection<ApiTemplateDto>> FindAll(ApiTemplateFilterDto filter);
        IQueryable<ApiTemplateDto> FindAllPaged(ApiTemplatePagedFilterDto filter);
        Task<bool> DeleteById(int id);
        Task<bool> CreateAsync(ApiTemplate template);
        Task<bool> UpdateAsync(ApiTemplate template);
        Task<bool> RemovePromptLinked(int templateId);
    }
}
