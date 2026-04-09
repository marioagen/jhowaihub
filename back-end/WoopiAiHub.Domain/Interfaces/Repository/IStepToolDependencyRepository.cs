using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolDependencyRepository
    {
        Task DeleteByStepToolIdAsync(IEnumerable<int> stepToolIds);
        Task CreateAsync(StepToolDependency dependency);
        Task<bool> CreateRangeAsync(List<StepToolDependency> dependencies);
        Task<ICollection<StepToolDependency>> FindByStepToolIdAsync(int stepToolId);
        Task<bool> HasDependenciesByStepToolIdsAsync(IEnumerable<int> stepToolIds);
    }
}
