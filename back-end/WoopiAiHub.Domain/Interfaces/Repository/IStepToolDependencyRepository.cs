using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolDependencyRepository
    {
        Task DeleteByStepToolIdAsync(IEnumerable<int> stepToolIds);
        Task CreateAsync(StepToolDependency dependency);
        Task<ICollection<StepToolDependency>> FindByStepToolIdAsync(int stepToolId);
    }
}
