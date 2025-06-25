using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;

namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface IAccountServices
    {
        Task<AccessDataAuthDto> Authenticate(AuthenticateDto authenticateDto,
                                             AuthenticateHeaderDto authenticateHeaderDto);

        string AuthenticateApi(string key);

        string FindClientId();
    }
}
