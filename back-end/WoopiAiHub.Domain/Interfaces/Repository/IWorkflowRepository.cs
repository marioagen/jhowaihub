using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IWorkflowRepository
    {
        Task<bool> Create(Workflow workflow);
        Task<bool> Update(Workflow workflow);
        Task<WorkflowDto?> FindByTeamId(int teamId);
        Task<WorkflowDto?> FindById(int id);
        Task<Workflow?> FindByIdReturnModel(int id);
        Task<bool> DeleteById(int id);
        ICollection<WorkflowDto> FindAllByUser(string userEmail);
    }
}
