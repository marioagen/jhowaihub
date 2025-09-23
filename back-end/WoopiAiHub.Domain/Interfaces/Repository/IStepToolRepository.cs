using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolRepository
    {
        ICollection<StepTool> FindStepToolsByStepId(int stepId);
    }
}
