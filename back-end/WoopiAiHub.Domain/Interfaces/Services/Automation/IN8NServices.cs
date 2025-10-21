using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.DTOs.Response.Automation;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IN8NServices
    {
        Task<AutomationServicesDto> ProcessMessage(AutomationOutputDto automationOutputDto);
    }
}
