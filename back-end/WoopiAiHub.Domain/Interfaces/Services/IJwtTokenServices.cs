
namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IJwtTokenServices
    {
        string GenerateTokenWithParameters(string? jwtKey, string jwtIssuer, string jwtAudience, string user, int? tokenExpirationTime = null);
    }
}