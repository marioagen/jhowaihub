using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IWorkflowServices
    {
        Task<WorkflowDto> FindByTeamId(int teamId, WorkflowFilterDto workflowFilterDto);
        Task<WorkflowDto> FindById(int id, WorkflowFilterDto? workflowFilterDto);
        Task<ICollection<StepDto>> FindStepsById(int id, WorkflowFilterDto? workflowFilterDto);
        Task<bool> DeleteById(int id);
        ICollection<WorkflowDto> FindAllByUser(string email);
        ICollection<WorkflowDto> FindAll();
        Task<ICollection<Workflow>> FindByProfileStep(ICollection<Profile> profiles);
        Task CreateWorkflowRelationship(Profile profile, List<int> stepId);
        Task UpdateTeamWorkflowRelationship(Team team, List<Workflow> workflows, List<Domain.Models.Profile> profiles);
        Task UpdateTeamProfileRelationshipToWorkflow(List<int> steps, Profile profile);
        PaginatedListDto<WorkflowDto> FindAllPaged(WorkflowPagedDto workflowPagedDto);
        Task<bool> UpdateStepToolOutput(OutputUpdateDto outputUpdateDto);
        Task RemoveTeamWorkflowRelationship(List<TeamsWorkflowsDto> teamsWorkflowsDto);
        Task<TeamsWorkflowsDto> VerifyWorkflowMatchInOtherTeamProfile(int profileId, int teamId, List<Workflow> workflows);
        Task<int> CreatePhase1(WorkflowPhase1Dto workflowPhase1Dto);
        Task<bool> UpdatePhase2(WorkflowPhase2Dto workflowPhase2Dto);
        Task<bool> UpdatePhase3(WorkflowPhase3Dto workflowPhase3Dto);
        Task <Phase1Dto> FindPhase1ById(int id);
        Task <List<StepDto>> FindPhase2ById(int id);
        Task <List<StepDto>> FindPhase3ById(int id);
        Task<bool> UpdatePhase1(WorkflowUpdatePhase1Dto workflowUpdatePhase1Dto);
        StepDto FindStepById(int id);
        Task<ICollection<ResponseWorkflowByDocumentDto>> FindWorkflowsByDocument(RequestWorkFlowByDocumentDto dto, CancellationToken ct = default);
    }
}
