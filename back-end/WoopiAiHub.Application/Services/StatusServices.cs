using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class StatusServices(IStatusRepository statusRepository) : IStatusServices
    {
        private readonly IStatusRepository _statusRepository = statusRepository;

        /// <summary>
        /// Retrieves all status.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<StatusDto>> FindAll()
        {
            return await _statusRepository.FindAll();
        }

        /// <summary>
        /// Retrieves the collection of status information associated with workflow steps.
        /// </summary>
        /// <remarks>This method calls the underlying repository to obtain status data. Ensure that the
        /// repository is properly initialized before invoking this method.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see
        /// cref="StatusDto"/> objects representing the statuses for workflow steps.</returns>
        public async Task<ICollection<StatusDto>> FindStatusForWorkflowSteps()
        {
            return await _statusRepository.FindStatusForWorkflowSteps();
        }
    }
}
