using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.ThirdParty
{
    public interface IThirdPartyApiHandler
    {
        Task<ThirdPartyApiResponseDto> ExecuteAsync(ThirdPartyApiRequestDto request, CancellationToken cancellationToken = default);
    }
}
