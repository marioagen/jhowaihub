using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolExecutionRepository
    {
        Task<bool> Create(StepToolExecution stepToolExecution);
        Task UpdateAsync(StepToolExecution stepToolExecution);
        Task<StepToolExecution> FindByStepToolIdAsync(int stepToolId);
    }
}
