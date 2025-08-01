using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Account;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAccountServices
    {
        Task<AccessDataAuthDto> Login(LoginDto loginDto);
        Task<AccessDataAuthDto> LoginSSO(AuthenticateDto authenticateDto, AuthenticateHeaderDto authenticateHeaderDto);
        string AuthenticateApi(string key);
        string FindClientId();
        Task<string?> RefreshTokenAsync(string refreshToken);
    }
}