using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class WorkflowServices : IWorkflowServices
    {
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowServices(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<WorkflowDto> FindById(int id)
        {
            var workflow = await _workflowRepository.FindById(id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, "Workflow not found", WorkflowLabel.NotFound);
            }
            return workflow;
        }

        /// <summary>
        /// Retrieves a workflow associated with a specific team ID.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<WorkflowDto> FindByTeamId(int teamId)
        {
            var workflow = await _workflowRepository.FindByTeamId(teamId);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, "Workflow not found", WorkflowLabel.NotFound);
            }
            return workflow;
        }
    }
}
