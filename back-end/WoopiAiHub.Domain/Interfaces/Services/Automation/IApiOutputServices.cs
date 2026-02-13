using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IApiOutputServices
    {
        Task<AutomationServicesDto> ProcessMessage(ApiOutputDto outputDto);
    }
}
