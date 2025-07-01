using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IUserServices
    {
        public Task<bool> Create(UserCreateDto userCreateDto,
                                 HeadersDto headersDto);

    }
}
