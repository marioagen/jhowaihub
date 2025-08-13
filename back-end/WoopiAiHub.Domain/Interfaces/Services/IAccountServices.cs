using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAccountServices
    {
        Task<AccessDataAuthDto> Authenticate(AuthenticateDto authenticateDto,
                                             AuthenticateHeaderDto authenticateHeaderDto);

        string AuthenticateApi(string key);

        string FindClientId();
    }
}
