using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Utils
{
    public class ValidateWorkflow : IValidateWorkflow
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ITeamRepository _teamRepository;

        public ValidateWorkflow(IWorkflowRepository workflowRepository, 
                                ITeamRepository teamRepository)
        {
            _workflowRepository = workflowRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Validates the creation of a new workflow.
        /// </summary>
        /// <param name="workflowCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task ValidateCreateWorkflow(WorkflowCreateDto workflowCreateDto)
        {
            if (string.IsNullOrEmpty(workflowCreateDto.Name))
            {
                throw new AppException(ErrorCode.RequiredField, "Workflow name cannot be empty", WorkflowLabel.NameRequired);
            }

            var workflowDto = await _workflowRepository.FindByTeamId(workflowCreateDto.TeamId, null);
            if (workflowDto != null)
            {
                throw new AppException(ErrorCode.Conflict, "Workflow already exists for this team", WorkflowLabel.AlreadyExists);
            }

            var team = _teamRepository.FindById(workflowCreateDto.TeamId);
            if (team == null)
            {
                throw new AppException(ErrorCode.NotFound, "Team not found", TeamLabel.NotFound);
            }
        }

        /// <summary>
        /// Validates the update of an existing workflow.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<WorkflowDto> ValidateUpdateWorkflow(WorkflowUpdateDto workflowUpdateDto)
        {
            if (string.IsNullOrEmpty(workflowUpdateDto.Name))
            {
                throw new AppException(ErrorCode.RequiredField, "Workflow name cannot be empty", WorkflowLabel.NameRequired);
            }

            var workflow = await _workflowRepository.FindByIdReturnModel(workflowUpdateDto.Id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, "Workflow not found", WorkflowLabel.NotFound);
            }

            if (workflow.TeamId != workflowUpdateDto.TeamId)
            {
                throw new AppException(ErrorCode.Conflict, "Workflow team ID does not match", WorkflowLabel.TeamIdMismatch);
            }

            return workflow;
        }
    }
}
