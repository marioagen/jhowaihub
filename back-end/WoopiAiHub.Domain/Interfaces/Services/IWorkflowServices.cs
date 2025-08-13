using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IWorkflowServices
    {
        Task<WorkflowDto> FindByTeamId(int teamId);
        Task<WorkflowDto> FindById(int id);
    }
}
