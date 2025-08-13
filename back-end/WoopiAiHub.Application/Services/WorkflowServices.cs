using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class WorkflowServices : IWorkflowServices
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateWorkflow _validateWorkflow;
        private readonly IValidateStep _validateStep;
        private const string NotFoundMessage = "Workflow not found";

        public WorkflowServices(IWorkflowRepository workflowRepository,
                                IProfileRepository profileRepository,
                                IStatusRepository statusRepository,
                                IStepRepository stepRepository,
                                IUnitOfWork unitOfWork,
                                IValidateStep validateStep,
                                IValidateWorkflow validateWorkflow)
        {
            _workflowRepository = workflowRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _stepRepository = stepRepository;
            _unitOfWork = unitOfWork;
            _validateStep = validateStep;
            _validateWorkflow = validateWorkflow;
        }

        /// <summary>
        /// Creates a new workflow for a specific team.
        /// </summary>
        /// <param name="workflowCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> Create(WorkflowCreateDto workflowCreateDto)
        {
            await _validateWorkflow.ValidateCreateWorkflow(workflowCreateDto);

            _validateStep.ValidateCreateStep(workflowCreateDto.Steps);

            var workflow = new Workflow(0, DateTime.UtcNow, workflowCreateDto.TeamId, workflowCreateDto.Name);

            ICollection<Step> steps = await CreateStepsAndValidate(workflowCreateDto.Steps, workflow.TeamId);

            workflow.AddSteps(steps);

            return await _workflowRepository.Create(workflow);
        }

        /// <summary>
        /// Updates an existing workflow.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> Update(WorkflowUpdateDto workflowUpdateDto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var workflow = await _validateWorkflow.ValidateUpdateWorkflow(workflowUpdateDto);

                _validateStep.ValidateUpdateStep(workflow, workflowUpdateDto.Steps);

                await DeleteSteps(workflowUpdateDto, workflow);

                await UpdateSteps(workflowUpdateDto);

                ICollection<Step> stepsAdd = await CreateStepsAndValidate(workflowUpdateDto.Steps.Where(s => s.Id == 0).ToList(), 
                                                                          workflow.TeamId);

                workflow.AddSteps(stepsAdd);

                await _workflowRepository.Update(workflow);

                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
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
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
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
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }
            return workflow;
        }

        /// <summary>
        /// Deletes a workflow by its ID, including all associated steps.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> DeleteById(int id)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var workflow = await _workflowRepository.FindByIdReturnModel(id);
                if (workflow == null)
                {
                    throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
                }

                var stepIds = workflow.Steps.Select(s => s.Id).ToList();
                await _validateStep.ValidateDeleteStep(stepIds);

                _stepRepository.DeleteByIds(stepIds);
                await _workflowRepository.DeleteById(id);
                
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Validates that the profile and status associated with a step exist.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<bool> ValidateProfileAndStatus(Step step)
        {
            var profile = await _profileRepository.FindById(step.ProfileId);
            if (profile == null)
            {
                throw new AppException(ErrorCode.NotFound, "Profile not found", ProfileLabel.NotFound);
            }
            var status = await _statusRepository.FindById(step.StatusId);
            if (status == null)
            {
                throw new AppException(ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);
            }
            return true;
        }

        /// <summary>
        /// Deletes steps that are no longer present in the updated workflow DTO.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task DeleteSteps(WorkflowUpdateDto workflowUpdateDto, Workflow workflow)
        {
            var updatedStepIds = workflowUpdateDto.Steps.Select(s => s.Id).ToHashSet();
            var stepsToRemove = workflow.Steps.Where(s => !updatedStepIds.Contains(s.Id)).Select(s => s.Id).ToList();

            await _validateStep.ValidateDeleteStep(stepsToRemove);
            _stepRepository.DeleteByIds(stepsToRemove);
        }

        /// <summary>
        /// Updates existing steps in a workflow based on the provided DTO.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <returns></returns>
        private async Task UpdateSteps(WorkflowUpdateDto workflowUpdateDto)
        {
            var stepsToUpdate = workflowUpdateDto.Steps.Where(s => s.Id > 0).ToList();

            foreach (var step in stepsToUpdate)
            {
                var existingStep = await _stepRepository.FindById(step.Id);
                if (existingStep != null)
                {
                    existingStep.Update(step.Name, step.Order, step.ProfileId, step.StatusId);

                    await ValidateProfileAndStatus(existingStep);
                    await _stepRepository.Update(existingStep);
                }
            }
        }

        /// <summary>
        /// Creates a collection of Step entities from the provided DTOs and associates them with the given teamId.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stepsDto"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        private async Task<ICollection<Step>> CreateStepsAndValidate<T>(IEnumerable<T> stepsDto, int teamId) where T : IStepDto
        {
            var steps = stepsDto.Select(s => new Step(
                0,
                DateTime.UtcNow,
                teamId,
                s.Name,
                s.Order,
                s.ProfileId,
                s.StatusId)).ToList();

            foreach(var step in steps)
            {
                await ValidateProfileAndStatus(step);
            }

            return steps;
        }
    }
}
