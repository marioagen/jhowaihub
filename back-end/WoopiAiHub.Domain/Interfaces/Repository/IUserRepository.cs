using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<bool> CreateAsync(User user);
        Task<User> FindByReferenceAsync(Guid referenceUserId);
        Task<List<User>> FindByIdsAsync(List<Guid> ids);
        bool DeactivateRange(List<Guid> ids);
        bool Update(User user);
        IQueryable<UserDtoPaged> FindAllPaged(PagedDataDto pagedDataDto);

    }
}
