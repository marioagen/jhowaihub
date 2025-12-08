using System;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUserServices
    {
        Task<bool> Create(UserCreateDto userCreateDto, HeadersDto headersDto);
        Task<bool> DeactivateRange(List<Guid> ids);
        Task<bool> Update(UserUpdateDto userUpdateDto,
                          HeadersDto headersDto);
        UserPagedResultDto FindAllPaged(PagedDataDto pagedDataDto);
        Task<bool> IsEmailInUseAsync(UserEmailDto userEmailDto);
        Task<ICollection<UserDto>> FindByTeamId(int teamId);
        Task<UserDto> FindUserByEmail(string email);
        Guid FindIdByEmail(string email);
        Task<ICollection<UserDto>> FindByTeamIds(int[] teamIds);
    }
}
