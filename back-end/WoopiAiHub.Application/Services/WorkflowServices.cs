using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

namespace WoopiAiHub.Application.Services
{
    public class WorkflowServices : IWorkflowServices
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IUnitOfWork _unitOfWork;
        private const string NotFoundMessage = "Workflow not found";

        public WorkflowServices(IWorkflowRepository workflowRepository,
                                IProfileRepository profileRepository,
                                IStatusRepository statusRepository,
                                ITeamRepository teamRepository,
                                IStepRepository stepRepository,
                                IUnitOfWork unitOfWork,
                                ICardRepository cardRepository)
        {
            _workflowRepository = workflowRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _teamRepository = teamRepository;
            _stepRepository = stepRepository;
            _unitOfWork = unitOfWork;
            _cardRepository = cardRepository;
        }

        /// <summary>
        /// Creates a new workflow for a specific team.
        /// </summary>
        /// <param name="workflowCreateDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> Create(WorkflowCreateDto workflowCreateDto)
        {
            var workflowDto = await _workflowRepository.FindByTeamId(workflowCreateDto.TeamId);
            if (workflowDto != null)
            {
                throw new AppException(ErrorCode.Conflict, "Workflow already exists for this team", WorkflowLabel.AlreadyExists);
            }

            var team = _teamRepository.FindById(workflowCreateDto.TeamId);
            if (team == null)
            {
                throw new AppException(ErrorCode.NotFound, "Team not found", TeamLabel.NotFound);
            }

            var workflow = new Workflow(0, DateTime.UtcNow, workflowCreateDto.TeamId, workflowCreateDto.Name);

            ICollection<Step> steps = workflowCreateDto.Steps.Select(s => new Step(
                0,
                DateTime.UtcNow,
                workflow.TeamId,
                s.Name,
                s.Order,
                s.ProfileId,
                s.StatusId)).ToList();

            await AddSteps(steps, workflow);

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
                var workflow = await _workflowRepository.FindByIdReturnModel(workflowUpdateDto.Id);
                if (workflow == null)
                {
                    throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
                }

                if (workflow.TeamId != workflowUpdateDto.TeamId)
                {
                    throw new AppException(ErrorCode.Conflict, "Workflow team ID does not match", WorkflowLabel.TeamIdMismatch);
                }

                await DeleteSteps(workflowUpdateDto, workflow);

                await UpdateSteps(workflowUpdateDto);

                ICollection<Step> stepsAdd = workflowUpdateDto.Steps
                    .Where(s => s.Id == 0)
                    .Select(s => new Step(
                        0,
                        DateTime.UtcNow,
                        workflow.TeamId,
                        s.Name,
                        s.Order,
                        s.ProfileId,
                        s.StatusId))
                    .ToList();

                await AddSteps(stepsAdd, workflow);

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

                List<int> stepsToRemove = await VerifyAndReturnSteps(workflow, workflow.Steps.Select(s => s.Id).ToHashSet());
                _stepRepository.DeleteByIds(stepsToRemove);

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
        /// Adds steps to a workflow, ensuring that each step has a valid profile and status.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="workflow"></param>
        /// <exception cref="AppException"></exception>
        private async Task AddSteps(ICollection<Step> steps, Workflow workflow)
        {
            workflow.Steps.Clear();
            foreach (var step in steps)
            {
                await ValidateProfileAndStatus(step);

                workflow.AddStep(step);
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
            List<int> stepsToRemove = await VerifyAndReturnSteps(workflow, updatedStepIds);
            _stepRepository.DeleteByIds(stepsToRemove);
        }

        /// <summary>
        /// Verifies which steps need to be removed and checks if they are in use by any cards.
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="updatedStepIds"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task<List<int>> VerifyAndReturnSteps(Workflow workflow, HashSet<int> updatedStepIds)
        {
            var stepsToRemove = workflow.Steps.Where(s => !updatedStepIds.Contains(s.Id)).Select(s => s.Id).ToList();
            var existingStepsInUse = await _cardRepository.ExistsStepsInUse(stepsToRemove);
            if (existingStepsInUse)
            {
                throw new AppException(ErrorCode.Conflict, "Cannot delete steps that are in use by cards", StepLabel.StepsInUse);
            }

            return stepsToRemove;
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
    }
}
