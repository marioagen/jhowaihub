using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IWorkflowRepository
    {
        Task<WorkflowDto?> FindByTeamId(int teamId);
        Task<WorkflowDto?> FindById(int id);
        Task<Workflow?> FindByIdReturnModel(int id);
    }
}
