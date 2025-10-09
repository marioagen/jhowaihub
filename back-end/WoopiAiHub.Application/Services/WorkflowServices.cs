using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.Repository;

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
        private readonly IStepToolRepository _stepToolRepository;
        private const string NotFoundMessage = "Workflow not found";

        public WorkflowServices(IWorkflowRepository workflowRepository,
                                IProfileRepository profileRepository,
                                IStatusRepository statusRepository,
                                IStepRepository stepRepository,
                                IUnitOfWork unitOfWork,
                                IValidateStep validateStep,
                                IValidateWorkflow validateWorkflow,
                                IStepToolRepository stepToolRepository)
        {
            _workflowRepository = workflowRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _stepRepository = stepRepository;
            _unitOfWork = unitOfWork;
            _validateStep = validateStep;
            _validateWorkflow = validateWorkflow;
            _stepToolRepository = stepToolRepository;

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

            ICollection<Step> steps = await CreateStepsAndValidate(workflowCreateDto.Steps, workflowCreateDto.TeamId);

            workflow.AddSteps(steps);

            return await _workflowRepository.Create(workflow);
        }

        /// <summary>
        /// Updates an existing workflow and its steps and step tools based on the provided data transfer object (DTO).
        /// <para>
        /// For each step in the DTO:
        /// - If the step exists, updates its properties and removes any step tools that are no longer present.
        /// - Updates existing step tools or creates new ones, maintaining correct order and dependencies.
        /// - If the step does not exist, creates it along with its step tools.
        /// </para>
        /// <para>
        /// Dependencies are handled so that the first step tool in a step depends on the last global step tool,
        /// and subsequent step tools in the same step depend on the previous step tool in the same step.
        /// </para>
        /// <para>
        /// All changes are wrapped in a transaction. If an exception occurs, the transaction is rolled back.
        /// </para>
        /// </summary>
        /// <param name="workflowUpdateDto">The DTO containing the workflow updates, including steps and step tools.</param>
        /// <returns>Returns true if the update is successful; otherwise, the transaction is rolled back and an exception is thrown.</returns>
        public async Task<bool> Update(WorkflowUpdateDto workflowUpdateDto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var workflow = await _workflowRepository.FindByIdReturnModel(workflowUpdateDto.Id);
                _validateStep.ValidateUpdateStep(workflow, workflowUpdateDto.Steps);
                workflow.Update(workflowUpdateDto.Name);

                StepTool? lastGlobalStepTool = null;

                foreach (var stepDto in workflowUpdateDto.Steps.OrderBy(s => s.Order))
                {
                    Step? existingStep = workflow.Steps.FirstOrDefault(s => s.Id == stepDto.Id);
                    StepTool? previousStepToolInStep = null;

                    if (existingStep != null)
                    {
                        existingStep.Update(stepDto.Name, stepDto.Order, stepDto.ProfileId, stepDto.StatusId);

                        var stepToolIdsFromDto = stepDto.StepTools.Select(st => st.Id).ToHashSet();
                        var stepToolsToRemove = existingStep.StepTools
                                                            .Where(st => !stepToolIdsFromDto.Contains(st.Id))
                                                            .ToList();
                        foreach (var stepToolToRemove in stepToolsToRemove)
                        {
                            var dependents = workflow.Steps.SelectMany(s => s.StepTools)
                                                           .Where(st => st.DependsOnStepToolId == stepToolToRemove.Id)
                                                           .ToList();

                            foreach (var dependent in dependents)
                                dependent.RemoveDependency();

                            stepToolToRemove.RemoveDependency();
                            existingStep.RemoveStepTool(stepToolToRemove);
                        }
                        await _unitOfWork.SaveChangesAsync();

                        foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                        {
                            var stepTool = existingStep.StepTools.FirstOrDefault(st => st.Id == stepToolDto.Id)
                                           ?? CreateStepToolUpdate(stepToolDto);

                            stepTool.Update(stepToolDto.ToolId, stepToolDto.Order, stepToolDto.PositionX, stepToolDto.PositionY, null);
                            stepTool.DependsOnStepTool = previousStepToolInStep ?? lastGlobalStepTool;

                            if (!existingStep.StepTools.Contains(stepTool))
                                existingStep.AddStepTool(stepTool);

                            previousStepToolInStep = stepTool;
                            lastGlobalStepTool = stepTool;
                        }
                    }
                    else
                    {
                        var newStep = CreateStep(stepDto, workflowUpdateDto.TeamId);
                        foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                        {
                            var stepTool = CreateStepToolUpdate(stepToolDto);
                            stepTool.DependsOnStepTool = previousStepToolInStep ?? lastGlobalStepTool;

                            newStep.AddStepTool(stepTool);
                            previousStepToolInStep = stepTool;
                            lastGlobalStepTool = stepTool;
                        }

                        workflow.AddStep(newStep);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
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
        public async Task<WorkflowDto> FindByTeamId(int teamId, WorkflowFilterDto workflowFilterDto)
        {
            var workflow = await _workflowRepository.FindByTeamId(teamId, workflowFilterDto);

            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            int totalCards = workflow.Steps.Sum(step => step.Cards.Count);
            workflow.NumDocuments = totalCards;

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
        /// Process StepTools deletion or inclusion 
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stepToolUpdateDtos"></param>
        /// <returns></returns>
        public async Task ProcessStepTools(Step step, ICollection<StepToolUpdateDto> stepToolUpdateDtos)
        {
            var stepToolsInsert = new List<StepTool>();
            foreach (var stepToolUpdate in stepToolUpdateDtos)
            {
                var stepTool = new StepTool(
                                            0,
                                            DateTime.Now,
                                            step.Id,
                                            stepToolUpdate.ToolId,
                                            stepToolUpdate.Order,
                                            stepToolUpdate.PositionX,
                                            stepToolUpdate.PositionY
                                        );
                if (stepToolUpdate.DependsOnStepToolId.HasValue)
                {
                    stepTool.UpdateDependencyStepToolId(stepToolUpdate.DependsOnStepToolId.Value);
                }

                if (!string.IsNullOrEmpty(stepToolUpdate.Input))
                {
                    stepTool.Parameters.Add(new StepToolParameter(0, DateTime.Now, 0, stepToolUpdate.Input));
                }

                stepToolsInsert.Add(stepTool);
            }

            await _stepToolRepository.CreateRangeAsync(stepToolsInsert);
        }

        /// <summary>
        /// Creates a collection of steps from the provided step DTOs, validates their profiles and statuses,  and
        /// establishes dependencies between step tools.
        /// </summary>
        /// <remarks>This method processes the provided step DTOs to create corresponding <see
        /// cref="Step"/> objects.  Each step is populated with its associated step tools, and dependencies between step
        /// tools are established  based on their order. After creation, the method validates the profile and status of
        /// each step.</remarks>
        /// <typeparam name="T">The type of the step DTO, which must implement <see cref="IStepDto"/>.</typeparam>
        /// <param name="stepsDto">A collection of step DTOs used to create the steps.</param>
        /// <param name="teamId">The identifier of the team to associate with the created steps.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
        /// cref="Step"/> objects created and validated from the provided DTOs.</returns>
        private async Task<ICollection<Step>> CreateStepsAndValidate<T>(IEnumerable<T> stepsDto, int teamId) where T : IStepDto
        {
            var steps = new List<Step>();
            StepTool? lastStepTool = null;


            foreach (var stepDto in stepsDto)
            {
                var step = CreateStep(stepDto, teamId);
                StepTool? previousStepToolInSameStep = null;

                foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                {
                    var stepTool = CreateStepToolUpdate(stepToolDto);
                    SetDependencies(stepTool, previousStepToolInSameStep, lastStepTool);

                    step.AddStepTool(stepTool);

                    previousStepToolInSameStep = stepTool;
                    lastStepTool = stepTool;
                }

                steps.Add(step);
            }

            foreach (var step in steps)
            {
                await ValidateProfileAndStatus(step);
            }

            return steps;
        }

        /// <summary>
        /// Sets the dependency for the specified <paramref name="stepTool"/> based on the provided context.
        /// </summary>
        /// <remarks>This method determines the dependency for <paramref name="stepTool"/> based on the
        /// provided parameters. If <paramref name="previousStepToolInSameStep"/> is provided, it takes precedence as
        /// the dependency. Otherwise, <paramref name="lastStepTool"/> is used if it is not null.</remarks>
        /// <param name="stepTool">The step tool for which the dependency is being set. This parameter cannot be null.</param>
        /// <param name="previousStepToolInSameStep">The previous step tool within the same step. If not null, this will be set as the dependency for <paramref
        /// name="stepTool"/>.</param>
        /// <param name="lastStepTool">The last step tool from a previous step. If <paramref name="previousStepToolInSameStep"/> is null and this
        /// parameter is not null, this will be set as the dependency for <paramref name="stepTool"/>.</param>
        private void SetDependencies(StepTool stepTool,
                                     StepTool? previousStepToolInSameStep,
                                     StepTool? lastStepTool)
        {
            if (previousStepToolInSameStep == null && lastStepTool != null)
            {
                stepTool.DependsOnStepTool = lastStepTool;
            }
            else if (previousStepToolInSameStep != null)
            {
                stepTool.DependsOnStepTool = previousStepToolInSameStep;
            }
        }

        /// <summary>
        /// Creates a new <see cref="StepTool"/> instance based on the provided update data.
        /// </summary>
        /// <remarks>If the <paramref name="stepToolDto"/> contains a non-empty <see
        /// cref="StepToolUpdateDto.Input"/> value, a corresponding <see cref="StepToolParameter"/> is added to the <see
        /// cref="StepTool.Parameters"/> collection.</remarks>
        /// <param name="stepToolDto">The data transfer object containing the update information for the <see cref="StepTool"/>.</param>
        /// <returns>A new <see cref="StepTool"/> instance initialized with the specified update data.</returns>
        private StepTool CreateStepToolUpdate(StepToolUpdateDto stepToolDto)
        {
            var stepTool = new StepTool(
                0,
                DateTime.Now,
                0,
                stepToolDto.ToolId,
                stepToolDto.Order,
                stepToolDto.PositionX,
                stepToolDto.PositionY);

            if (!string.IsNullOrEmpty(stepToolDto.Input))
            {
                stepTool.Parameters.Add(
                    new StepToolParameter(0, DateTime.Now, 0, stepToolDto.Input));
            }

            return stepTool;
        }

        /// <summary>
        /// Creates a new <see cref="Step"/> instance with the specified details.
        /// </summary>
        /// <param name="stepDto">An object containing the data required to initialize the step, including its name, order, profile ID, and
        /// status ID.</param>
        /// <param name="teamId">The identifier of the team associated with the step.</param>
        /// <returns>A new <see cref="Step"/> instance initialized with the provided data.</returns>
        private Step CreateStep(IStepDto stepDto, int teamId)
        {
            return new Step(
                0,
                DateTime.UtcNow,
                teamId,
                stepDto.Name,
                stepDto.Order,
                stepDto.ProfileId,
                stepDto.StatusId);
        }

        /// <summary>
        /// Find all workflows associated with a user, based on their email.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICollection<WorkflowDto> FindAllByUser(string email)
        {
            var workflow = _workflowRepository.FindAllByUser(email);
            return workflow;
        }
    }
}
