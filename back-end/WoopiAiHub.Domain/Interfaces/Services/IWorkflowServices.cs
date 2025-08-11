using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IWorkflowServices
    {
        Task<bool> Create(WorkflowCreateDto workflowCreateDto);
        Task<bool> Update(WorkflowUpdateDto workflowUpdateDto);
        Task<WorkflowDto> FindByTeamId(int teamId);
        Task<WorkflowDto> FindById(int id);
    }
}
