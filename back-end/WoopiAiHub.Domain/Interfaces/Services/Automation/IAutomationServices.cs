using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Connector;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IAutomationServices
    {
        Task<bool> PrepareExecutionAsync(ICollection<Workflow> workflows);
        Task StartExecutionByWorkflowsAsync(AutomationServicesDto automationServicesDto, 
                                            ICollection<Workflow> workflows);
        Task StartExecutionByStepAsync(Step step, AutomationServicesDto automationServicesDto);
        Task StartExecutionByCardAsync(AutomationServicesDto automationServicesDto);
        Task ContinueExecution(AutomationServicesDto automationServicesDto);
        Task<ICollection<ConnectorDto>> FindN8nWorkflowsByToolId(int toolId);
        Task<ICollection<FormFieldDto>> FindN8nWebhookInputs(WebhookInputDto webhookInputDto);
    }
}
