using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IAutomationServices
    {
        void PrepareExecutionAsync(ICollection<Workflow> workflows);
        Task StartExecutionByWorkflowsAsync(ICollection<Workflow> workflows);
        Task StartExecutionByStepAsync(Step step);
        Task StartExecutionByCardAsync(int stepId, int cardId);
        Task ContinueExecution(int stepToolId, int cardId);
        ICollection<StepTool> FindStepToolsByStepId(int stepId);
        ICollection<StepToolDto> FindAll();
        Task<StepToolDto> FindById(int id);
        bool DeleteByIds(List<int> ids);
        Task<bool> Update(int id,
                          string input);
        Task<bool> CreateAsync(StepToolCreateDto stepToolCreateDto);
    }
}
