using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolDependencyRepository
    {
        /// <summary>
        /// Deletes all dependencies for a given StepTool
        /// </summary>
        /// <param name="stepToolId">The ID of the StepTool whose dependencies should be deleted</param>
        Task DeleteByStepToolIdAsync(int stepToolId);

        /// <summary>
        /// Adds a new dependency
        /// </summary>
        /// <param name="dependency">The dependency to add</param>
        Task AddAsync(StepToolDependency dependency);

        /// <summary>
        /// Gets all dependencies for a given StepTool
        /// </summary>
        /// <param name="stepToolId">The ID of the StepTool</param>
        /// <returns>Collection of dependencies</returns>
        Task<ICollection<StepToolDependency>> GetByStepToolIdAsync(int stepToolId);
    }
}
