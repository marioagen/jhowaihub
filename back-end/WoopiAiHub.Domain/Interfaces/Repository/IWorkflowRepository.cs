using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IWorkflowRepository
    {
        Task<bool> Create(Workflow workflow);
        Task<bool> Update(Workflow workflow);
        Task<bool> UpdateRange(ICollection<Workflow> workflows);
        Task<WorkflowDto?> FindByTeamId(int teamId, WorkflowFilterDto? workflowFilterDto);
        Task<WorkflowDto?> FindById(int id, WorkflowFilterDto? workflowFilterDto);
        Task<Workflow?> FindByIdReturnModel(int id);
        Task<Workflow?> FindByIdForFlow(int id);
        Task<bool> DeleteById(int id);
        ICollection<WorkflowDto> FindAllByUser(string userEmail);
        ICollection<WorkflowDto> FindAll();
        Task<ICollection<Workflow>> FindByStep(List<int> stepIds);
        Task<ICollection<Workflow>> FindByTeams(List<int> teamsIds);
        Task<List<Workflow>> FindByIdsAsync(ICollection<int> ids);
        IQueryable<WorkflowDto> FindAllWithFilter(WorkflowPagedDto workflowPagedDto);
        Task<bool> UpdateStepToolOutput(StepToolOutput stepToolOutput);
        StepToolOutput FindByStepToolOutputById(int id);
        Task<List<StepDto>> FindPhase2ById(int id);
        Task<List<StepDto>> FindPhase3ById(int id);
        Task<Phase1Dto> FindPhase1ById(int id);
        StepDto FindStepById(int id);
        Task<ToolDto> FindToolByStepToolId(int id);
        Task<ICollection<ResponseWorkflowByDocumentDto>> FindWorkflowsByDocument(RequestWorkFlowByDocumentDto dto, CancellationToken ct = default);
    }
}
