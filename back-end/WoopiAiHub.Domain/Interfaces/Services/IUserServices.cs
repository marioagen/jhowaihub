using Microsoft.AspNetCore.Mvc;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUserServices
    {
        public Task<bool> Create(UserCreateDto userCreateDto,
                                 HeadersDto headersDto);
        public Task<bool> DeactivateRange(List<Guid> ids);

        public Task<bool> Update(UserUpdateDto userUpdateDto,
                                       HeadersDto headersDto);

        public UserPagedResultDto FindAllPaged(PagedDataDto pagedDataDto);
    }
}
