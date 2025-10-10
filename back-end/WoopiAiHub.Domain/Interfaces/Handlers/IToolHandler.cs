using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Models;
namespace WoopiAiHub.Domain.Interfaces.Handlers
{
    public interface IToolHandler
    {
        Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                               StepToolParameter? input,
                                               string output);
    }
}