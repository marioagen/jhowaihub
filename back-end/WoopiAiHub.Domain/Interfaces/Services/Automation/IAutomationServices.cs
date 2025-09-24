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
    }
}
