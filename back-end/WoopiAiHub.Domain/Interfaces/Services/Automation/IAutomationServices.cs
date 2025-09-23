using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IAutomationServices
    {
        Task PrepareExecution(ICollection<Workflow> workflows);
        Task StartExecutionByWorkflows(ICollection<Workflow> workflows);
        Task StartExecutionByStep(Step step);
    }
}
