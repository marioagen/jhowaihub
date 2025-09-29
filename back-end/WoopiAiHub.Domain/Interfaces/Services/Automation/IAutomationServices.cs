using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IAutomationServices
    {
        Task<bool> PrepareExecutionAsync(ICollection<Workflow> workflows);
        Task StartExecutionByWorkflowsAsync(string tenant, string referenceFile, string email, ICollection<Workflow> workflows);
        Task StartExecutionByStepAsync(Step step, string tenant, string referenceFile, string email);
        Task StartExecutionByCardAsync(int stepId, int cardId, string tenant, string referenceFile, string email);
        Task ContinueExecution(int stepToolId, int cardId, string tenant, string email, string referenceFile);
    }
}
