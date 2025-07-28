using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IProfileRepository
    {
        Task<List<Profile>> FindByIdsAsync(IEnumerable<int> ids);
        IQueryable<ProfileDto> FindAllPaged(PagedDataDto pagedDataDto);
    }
}
