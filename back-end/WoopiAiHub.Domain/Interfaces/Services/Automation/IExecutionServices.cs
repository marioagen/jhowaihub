using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services.Automation
{
    public interface IExecutionServices
    {
        Task HandleExecutionProgress(StepToolExecution execution, string email);
    }
}
