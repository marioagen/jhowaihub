using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IProfileRepository
    {
        bool CreateUniqueProfile(Profile team);
        bool ExistsProfileByNameExceptId(string name, int excludeId);
        Task<ICollection<ProfileDto>> FindAll();
        Task<ProfileDto?> FindById(int id);
        bool Update(Profile team);
        Task<bool> DeleteByIdsAsync(List<int> ids);
        IQueryable<ProfileDto> FindAllPaged(PagedDataDto pagedDataDto);
        ICollection<Profile> FindByIds(IEnumerable<int> ids);
        Profile FindByIdReturnModel(int id);
    }
}
