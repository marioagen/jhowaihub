using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IApiTemplateRepository
    {
        Task<ApiTemplateDto?> FindById(Guid id);
        Task<ApiTemplate?> FindByIdReturnModel(Guid id);
        Task<ICollection<ApiTemplate>> FindAll(ApiTemplateFilterDto filter);
        IQueryable<ApiTemplateDto> FindAllPaged(ApiTemplatePagedFilterDto filter);
        Task<bool> DeleteById(Guid id);
        Task<bool> CreateAsync(ApiTemplate template);
        Task<bool> UpdateAsync(ApiTemplate template);
    }
}
