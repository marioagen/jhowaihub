using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolOutputRepository
    {
        Task<bool> CreateAsync(StepToolOutput stepToolOutput);
        Task<string> FindByStepToolId(int stepToolId);
    }
}
