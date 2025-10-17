using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request.Automation;
namespace WoopiAiHub.Domain.Interfaces.Handlers
{
    public interface IToolHandler
    {
        string Type { get; }
        Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                               string input,
                                               string output);
    }
}