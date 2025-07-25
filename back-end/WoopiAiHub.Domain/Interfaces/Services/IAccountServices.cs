using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Account;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAccountServices
    {
        Task<AccessDataAuthDto> Authenticate(AuthenticateDto authenticateDto, AuthenticateHeaderDto authenticateHeaderDto);
        string AuthenticateApi(string key);
        string FindClientId();
        Task<LoginResponseDto> Login(LoginDto loginDto);
    }
}
