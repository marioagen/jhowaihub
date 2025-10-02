using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IValidateWorkflow
    {
        Task ValidateCreateWorkflow(WorkflowCreateDto workflowCreateDto);
        Task<Workflow> ValidateUpdateWorkflow(WorkflowUpdateDto workflowUpdateDto);
    }
}
