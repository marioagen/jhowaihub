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
        Task<User> FindByEmailAsync(string email);
        bool DeactivateRange(List<Guid> ids);
        bool Update(User user);
        IQueryable<UserPagedDto> FindAllPaged(PagedDataDto pagedDataDto);
        Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
        Task<List<string>> FindUserProfilesByEmailAsync(string email);
        Task<ICollection<UserDto>> FindByTeamIdAsync(int teamId);
        Task<UserDto?> FindUserByEmail(string email);
        Guid FindIdByEmail(string email);
        Task<ICollection<UserDto>> FindByTeamIdsAsync(int[] teamIds);
        Task<ICollection<UserDto>> FindAllAsync();
    }
}
