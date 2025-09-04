using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IProfileRepository
    {
        bool CreateUniqueProfile(Profile team);
        Task<ICollection<ProfileDto>> FindAll();
        Task<ProfileDto?> FindById(int id);
        bool Update(Profile team);
        bool DeleteByIds(List<int> ids);
        IQueryable<ProfileDto> FindAllPaged(PagedDataDto pagedDataDto);
        ICollection<Profile> FindByIds(IEnumerable<int> ids);
        Profile FindByIdReturnModel(int id);
    }
}
