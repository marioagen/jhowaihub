using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IWorkflowServices
    {
        Task<bool> Create(WorkflowCreateDto workflowCreateDto);
        Task<bool> Update(WorkflowUpdateDto workflowUpdateDto);
        Task<WorkflowDto> FindByTeamId(int teamId, WorkflowFilterDto workflowFilterDto);
        Task<WorkflowDto> FindById(int id, WorkflowFilterDto? workflowFilterDto);
        Task<bool> DeleteById(int id);
        ICollection<WorkflowDto> FindAllByUser(string email);
        ICollection<WorkflowDto> FindAll();
        Task<ICollection<Workflow>> FindByProfileStep(ICollection<Profile> profiles);
        Task UpdateTeamProfileRelationshipToWorkflow(List<int> steps, Profile profile);
        PaginatedListDto<WorkflowDto> FindAllPaged(WorkflowPagedDto workflowPagedDto);
        Task<bool> UpdateStepToolOutput(OutputUpdateDto outputUpdateDto);
        Task RemoveTeamWorkflowRelationship(List<TeamsWorkflowsDto> teamsWorkflowsDto);
        Task<TeamsWorkflowsDto> VerifyWorkflowMatchInOtherTeamProfile(int profileId, int teamId, List<Workflow> workflows);
    }
}
