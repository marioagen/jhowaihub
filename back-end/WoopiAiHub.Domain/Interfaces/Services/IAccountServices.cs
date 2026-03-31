using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Request.Account;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAccountServices
    {
        Task<object> Login(LoginDto loginDto);
        Task<object> LoginSSO(AuthenticateDto authenticateDto, AuthenticateHeaderDto authenticateHeaderDto);
        string AuthenticateApi(string key);
        string FindClientId();
        Task<string?> RefreshTokenAsync(string refreshToken, string headerTenant);
        Task<bool> RevokeTokenAsync(string refreshToken);
        string GenerateToken(string user, int? tokenExpirationTime = null);
    }
}