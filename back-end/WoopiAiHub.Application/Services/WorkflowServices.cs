using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
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
        private readonly ITeamRepository _teamRepository;
        private readonly IStepRepository _stepRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IStepToolDependencyRepository _stepToolDependencyRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateStep _validateStep;
        private readonly ILogger<WorkflowServices> _logger;
        private const string NotFoundMessage = "Workflow not found";

        public WorkflowServices( IWorkflowRepository workflowRepository, IProfileRepository profileRepository,
            ITeamRepository teamRepository, IStatusRepository statusRepository,
            IStepRepository stepRepository,IStepToolDependencyRepository stepToolDependencyRepository,
            IStepToolOutputRepository stepToolOutputRepository, IUnitOfWork unitOfWork, IValidateStep validateStep,
            ILogger<WorkflowServices> logger
        )
        {
            _workflowRepository = workflowRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _stepRepository = stepRepository;
            _stepToolDependencyRepository = stepToolDependencyRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _unitOfWork = unitOfWork;
            _validateStep = validateStep;
            _teamRepository = teamRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<WorkflowDto> FindById(int id, WorkflowFilterDto? workflowFilterDto)
        {
            var workflow = await _workflowRepository.FindById(id, workflowFilterDto);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            int totalCards = workflow.Steps.Sum(step => step.Cards.Count);
            workflow.NumDocuments = totalCards;
            return workflow;
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<Phase1Dto> FindPhase1ById(int id)
        {
            var phase1 = await _workflowRepository.FindPhase1ById(id);
            if (phase1 == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }
            return phase1;
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<List<StepDto>> FindPhase2ById(int id)
        {
            var workflow = await _workflowRepository.FindPhase2ById(id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            return workflow;
        }

        /// <summary>
        /// Retrieves a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<List<StepDto>> FindPhase3ById(int id)
        {
            var workflow = await _workflowRepository.FindPhase3ById(id);
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
            var workflow = await _workflowRepository.FindByIdReturnModel(id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            var stepIds = workflow.Steps.Select(s => s.Id).ToList();
            await _validateStep.ValidateDeleteStep(stepIds);
            return await _workflowRepository.DeleteById(id);
        }

        /// <summary>
        /// This method sends the current page  
        /// and search text to repository and return an PaginatedListDto<WorkflowDto>.
        /// </summary>
        /// <param name="WorkflowPagedDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public PaginatedListDto<WorkflowDto> FindAllPaged(WorkflowPagedDto workflowPagedDto)
        {
            if (workflowPagedDto.Page > 0)
            {
                var workflowList = _workflowRepository.FindAllWithFilter(workflowPagedDto);
                return PaginationHelper.Paginate(workflowList, workflowPagedDto.Page);
                // return paginatedList;
            }
            else
            {
                var ex = new ArgumentException("Invalid Page");
                _logger.LogError(ex,
                    $"An argument exception occurred in the {nameof(WorkflowServices)} in the {nameof(FindAllPaged)} method");
                throw ex;
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
        private static StepTool CreateStepToolUpdate(StepToolUpdateDto stepToolDto)
        {
            var stepTool = new StepTool(
                0,
                DateTime.Now,
                0,
                stepToolDto.ToolId,
                stepToolDto.Order,
                stepToolDto.PositionX,
                stepToolDto.PositionY
            );

            foreach (var parameter in stepToolDto.Parameters)
            {
                stepTool.Parameters.Add(new StepToolParameter(0, DateTime.Now, 0, parameter.RequiredFile, parameter.WebhookId, parameter.Value));
            }

            return stepTool;
        }

        /// <summary>
        /// Find all workflows associated with a user, based on their email.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ICollection<WorkflowDto> FindAllByUser(string email)
        {
            var workflows = _workflowRepository.FindAllByUser(email);
            return workflows;
        }

        /// <summary>
        /// Find all workflows associated with a step.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Workflow>> FindByProfileStep(ICollection<Domain.Models.Profile> profiles)
        {
            var steps = profiles
                .SelectMany(p => p.StepProfilePermissions)
                .Select(s => s.StepId)
                .Distinct()
                .ToList();
            return await _workflowRepository.FindByStep(steps);
        }

        /// <summary>
        /// Creates workflow relationships for all teams associated with the given profile.
        /// It retrieves workflows based on the provided step IDs, then links each workflow
        /// to every team that belongs to the profile.
        /// </summary>
        public async Task CreateWorkflowRelationship(Domain.Models.Profile profile, List<int> stepsIds)
        {
            var workflows = await _workflowRepository.FindByStep(stepsIds);
            var profileTeams = workflows.SelectMany(w => w.Teams);

            foreach (var team in profileTeams)
            {
                await CreateRelationshipBetweenTeamWorkfloFromProfile(team.Id, workflows.ToList());
            }
        }

        /// <summary>
        /// Creates the relationship between a single team and a list of workflows.
        /// It loads the team, adds each workflow to it, and persists the update.
        /// </summary>
        private async Task CreateRelationshipBetweenTeamWorkfloFromProfile(int teamId, List<Workflow> workflows)
        {
            var team = _teamRepository.FindByIdReturnModel(teamId);
            foreach (var workflow in workflows)
            {
                team.AddWorkflow(workflow);
            }

            _teamRepository.Update(team);
        }

        /// <summary>
        /// Updates the relationship between a team and a collection of workflows based on the specified profiles.
        /// </summary>
        /// <remarks>This method verifies if the specified workflows are associated with other profiles
        /// within the team and removes any conflicting relationships. Only workflows that are distinct and relevant to
        /// the team are retained.</remarks>
        /// <param name="team">The team for which the workflow relationships are being updated.</param>
        /// <param name="workflows">A list of workflows to associate with the team.</param>
        /// <param name="profiles">A list of profiles used to verify and adjust workflow relationships.</param>
        /// <returns></returns>
        public async Task UpdateTeamWorkflowRelationship(
            Team team,
            List<Workflow> workflows,
            List<Domain.Models.Profile> profiles
        )
        {
            var workflowsToRemove = new List<TeamsWorkflowsDto>();
            foreach (var profile in profiles)
            {
                var teamsWorkflows = await VerifyWorkflowMatchInOtherTeamProfile(profile.Id, team.Id, workflows);
                workflowsToRemove.Add(teamsWorkflows);
            }

            var filterEmptyWorkflows = workflowsToRemove
                .Where(w => w.Workflows.Count > 0)
                .Select(w => new TeamsWorkflowsDto
                {
                    TeamId = w.TeamId,
                    Workflows = w.Workflows.Distinct().ToList()
                })
                .ToList();

            if (filterEmptyWorkflows.Count() > 0)
            {
                await RemoveTeamWorkflowRelationship(filterEmptyWorkflows);
            }
        }

        /// <summary>
        /// Update team-profiles with its workflows.
        /// </summary>
        /// <param name="list<int>"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task UpdateTeamProfileRelationshipToWorkflow(List<int> steps, Domain.Models.Profile profile)
        {
            var profileId = profile.Id;
            var profileTeams = profile.Teams;
            var workflows = await _workflowRepository.FindByStep(steps);

            if (workflows.Count() == 0)
                return;

            var workflowsToRemove = new List<TeamsWorkflowsDto>();
            foreach (var team in profileTeams)
            {
                var teamsWorkflows =
                    await VerifyWorkflowMatchInOtherTeamProfile(profileId, team.Id, workflows.ToList());
                workflowsToRemove.Add(teamsWorkflows);
            }

            var filterEmptyWorkflows = workflowsToRemove
                .Where(w => w.Workflows.Count > 0)
                .Select(w => new TeamsWorkflowsDto
                {
                    TeamId = w.TeamId,
                    Workflows = w.Workflows.Distinct().ToList()
                })
                .ToList();

            if (filterEmptyWorkflows.Count() > 0)
            {
                await RemoveTeamWorkflowRelationship(filterEmptyWorkflows);
            }
        }

        /// <summary>
        /// verify other profiles for a matching workflow.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TeamsWorkflowsDto> VerifyWorkflowMatchInOtherTeamProfile(int profileId, int teamId, List<Workflow> workflows)
        {
            var team = _teamRepository.FindByIdReturnModel(teamId);
            var profiles = team.Profiles.Where(p => p.Id != profileId).ToList();

            if (!profiles.Any())
                return new TeamsWorkflowsDto
                {
                    TeamId = teamId,
                    Workflows = workflows.Select(w => w.Id).ToList()
                };

            var workflowIds = workflows.Select(w => w.Id).ToHashSet();
            var workflowsFound = new HashSet<int>();

            foreach (var profile in profiles)
            {
                if (profile.StepProfilePermissions == null)
                    continue;

                var stepIds = profile.StepProfilePermissions
                    .Select(spp => spp.StepId)
                    .ToList();

                if (!stepIds.Any())
                    continue;

                var workflowsFromSteps = await _workflowRepository.FindByStep(stepIds);
                var workflowsFromStepIds = workflowsFromSteps.Select(w => w.Id).ToHashSet();

                foreach (var id in workflowIds)
                {
                    if (workflowsFromStepIds.Contains(id))
                        workflowsFound.Add(id);
                }
            }

            var workflowsNotFound = workflowIds.Except(workflowsFound).ToList();

            return new TeamsWorkflowsDto
            {
                TeamId = teamId,
                Workflows = workflowsNotFound,
            };
        }

        /// <summary>
        /// Remove relationship for workflows and teams
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveTeamWorkflowRelationship(List<TeamsWorkflowsDto> teamsWorkflowsDto)
        {
            foreach (var teamsWorkflows in teamsWorkflowsDto)
            {
                var team = _teamRepository.FindByIdReturnModel(teamsWorkflows.TeamId);
                if (team == null)
                    continue;

                var workflows = await _workflowRepository.FindByIdsAsync(teamsWorkflows.Workflows);
                if (workflows == null || !workflows.Any())
                    continue;

                team.RemoveWorkflows(workflows);
                _teamRepository.Update(team);
            }
        }

        /// <summary>
        /// Find all workflows.
        /// </summary>
        /// <returns></returns>
        public ICollection<WorkflowDto> FindAll()
        {
            var workflow = _workflowRepository.FindAll();
            return workflow;
        }

        /// <summary>
        /// Update output of step in a workflow by id.
        /// </summary>
        /// <param name="stepId"></param>
        ///<returns></returns>
        public async Task<bool> UpdateStepToolOutput(OutputUpdateDto outputUpdateDto)
        {
            var stepToolOutput = this.FindByStepToolOutputById(outputUpdateDto.Id);
            stepToolOutput.ChangeValue(outputUpdateDto.Value);
            var result = await _workflowRepository.UpdateStepToolOutput(stepToolOutput);
            return result;
        }

        /// <summary>
        ///  Find StepToolOutput by step id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StepToolOutput FindByStepToolOutputById(int id)
        {
            var stepToolOutput = _workflowRepository.FindByStepToolOutputById(id);
            return stepToolOutput;
        }

        /// <summary>
        /// Phase 1: Creates a workflow with name and team associations only.
        /// Returns the ID of the created workflow to be used in subsequent phases.
        /// </summary>
        /// <param name="workflowPhase1Dto"></param>
        /// <returns>The ID of the created workflow</returns>
        public async Task<int> CreatePhase1(WorkflowPhase1Dto workflowPhase1Dto)
        {
            if (string.IsNullOrWhiteSpace(workflowPhase1Dto.Name))
            {
                throw new AppException(ErrorCode.RequiredField, "Workflow name is required", WorkflowLabel.InvalidName);
            }

            if (workflowPhase1Dto.Teams == null || workflowPhase1Dto.Teams.Count == 0)
            {
                throw new AppException(ErrorCode.RequiredField, "At least one team must be selected",
                    WorkflowLabel.InvalidTeams);
            }

            var teamsList = _teamRepository.FindByIds(workflowPhase1Dto.Teams);
            if (teamsList.Count != workflowPhase1Dto.Teams.Count)
            {
                throw new AppException(ErrorCode.NotFound, "One or more teams not found", TeamLabel.NotFound);
            }

            var workflow = new Workflow(0, DateTime.UtcNow, teamsList, workflowPhase1Dto.Name);
            await _workflowRepository.Create(workflow);

            return workflow.Id;
        }

        /// <summary>
        /// Find stepDto by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StepDto FindStepById(int id)
        {
            var step = _workflowRepository.FindStepById(id);
            return step;
        }

        /// <summary>
        /// Phase 2: Updates a workflow with steps information (without step tools).
        /// Validates and creates/updates steps with their profiles and statuses.
        /// </summary>
        /// <param name="workflowPhase2Dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePhase2(WorkflowPhase2Dto workflowPhase2Dto)
        {
            var workflow = await _workflowRepository.FindByIdReturnModel(workflowPhase2Dto.WorkflowId);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var existingSteps = workflow.Steps.ToList();

                var newStepsDict = workflowPhase2Dto.Steps
                    .Where(s => s.Id > 0)
                    .ToDictionary(s => s.Id);

                var stepsToRemove = existingSteps
                    .Where(es => !newStepsDict.ContainsKey(es.Id))
                    .ToList();

                var stepsToUpdate = existingSteps
                    .Where(es => newStepsDict.ContainsKey(es.Id))
                    .ToList();

                var stepsToAdd = workflowPhase2Dto.Steps
                    .Where(s => s.Id == 0 || !existingSteps.Any(es => es.Id == s.Id))
                    .ToList();


                var stepcards = _stepRepository.FindByIdsWithCards(stepsToRemove.Select(s => s.Id));
                if (stepcards.Any(s => s.Cards.Count > 0))
                {
                    throw new AppException(ErrorCode.DefaultError, "Can't delete with cards related", null);
                }

                _stepRepository.DeleteByIds(stepsToRemove.Select(s => s.Id));

                foreach (var stepDto in workflowPhase2Dto.Steps.Where(s => s.Id > 0))
                {
                    var existingStep = stepsToUpdate.FirstOrDefault(s => s.Id == stepDto.Id);
                    if (existingStep != null)
                    {
                        await ValidateProfileAndStatusStepPhase2(stepDto);

                        existingStep.Update(stepDto.Name, stepDto.Order, stepDto.ProfileId, stepDto.StatusId);
                    }
                }

                foreach (var stepDto in stepsToAdd.OrderBy(s => s.Order))
                {
                    await ValidateProfileAndStatusStepPhase2(stepDto);

                    var newStep = new Step(
                        id: 0,
                        created: DateTime.Now,
                        workflowId: workflow.Id,
                        name: stepDto.Name,
                        order: stepDto.Order,
                        profileId: stepDto.ProfileId,
                        statusId: stepDto.StatusId
                    );

                    workflow.AddStep(newStep);
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
        /// Phase 2: Updates a workflow with steps information (without step tools).
        /// Validates and creates/updates steps with their profiles and statuses.
        /// </summary>
        /// <param name="workflowPhase2Dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePhase1(WorkflowUpdatePhase1Dto workflowUpdatePhase1Dto)
        {
            var workflow = await _workflowRepository.FindByIdReturnModel(workflowUpdatePhase1Dto.Id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var teamsList = _teamRepository.FindByIds(workflowUpdatePhase1Dto.Teams);
                foreach (var team in teamsList)
                {
                    workflow.AddTeam(team);
                }

                workflow.Update(workflowUpdatePhase1Dto.Name);
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
        /// Validates that the profile and status associated with a step DTO exist.
        /// </summary>
        /// <param name="stepDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        private async Task ValidateProfileAndStatusStepPhase2(StepPhase2Dto stepDto)
        {
            var profile = await _profileRepository.FindById(stepDto.ProfileId);
            if (profile == null)
            {
                throw new AppException(ErrorCode.NotFound, "Profile not found", ProfileLabel.NotFound);
            }

            var status = await _statusRepository.FindById(stepDto.StatusId);
            if (status == null)
            {
                throw new AppException(ErrorCode.NotFound, "Status not found", StatusLabel.NotFound);
            }
        }

        /// <summary>
        /// Phase 3: Updates workflow steps with their tool flows (step tools).
        /// Handles dependencies between step tools.
        /// </summary>
        /// <param name="workflowPhase3Dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePhase3(WorkflowPhase3Dto workflowPhase3Dto)
        {
            var workflow = await _workflowRepository.FindByIdReturnModel(workflowPhase3Dto.WorkflowId);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var stepToolMap = await ProcessStepTools(workflow, workflowPhase3Dto.Steps);
                await ResolveDependencies(workflow, workflowPhase3Dto.Steps, stepToolMap);

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
        /// Processes step tools for each step in the workflow.
        /// </summary>
        private async Task<Dictionary<(int stepId, int order), StepTool>> ProcessStepTools(
            Workflow workflow,
            ICollection<StepPhase3Dto> steps)
        {
            StepTool? lastGlobalStepTool = null;
            var stepToolMap = new Dictionary<(int stepId, int order), StepTool>();

            foreach (var stepDto in steps.OrderBy(s => s.Order))
            {
                var existingStep = FindStepInWorkflow(workflow, stepDto);
                await ClearExistingStepTools(existingStep);

                StepTool? previousStepToolInStep = null;

                foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                {
                    var stepTool = CreateAndConfigureStepTool(
                        stepToolDto,
                        existingStep,
                        previousStepToolInStep,
                        lastGlobalStepTool);

                    stepToolMap[(existingStep.Id, stepToolDto.Order)] = stepTool;
                    existingStep.AddStepTool(stepTool);

                    previousStepToolInStep = stepTool;
                    lastGlobalStepTool = stepTool;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return stepToolMap;
        }

        /// <summary>
        /// Resolves dependencies between step tools.
        /// </summary>
        private async Task ResolveDependencies(
            Workflow workflow,
            ICollection<StepPhase3Dto> steps,
            Dictionary<(int stepId, int order), StepTool> stepToolMap)
        {
            foreach (var stepDto in steps.OrderBy(s => s.Order))
            {
                var existingStep = workflow.Steps.FirstOrDefault(s => s.Id == stepDto.Id || s.Order == stepDto.Order);
                if (existingStep == null)
                    continue;

                foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                {
                    if (!stepToolMap.TryGetValue((existingStep.Id, stepToolDto.Order), out var stepTool))
                        continue;

                    await CreateDependenciesForStepTool(workflow, stepTool, stepToolDto);
                }
            }
        }

        /// <summary>
        /// Finds a step in the workflow by ID or order.
        /// </summary>
        private Step FindStepInWorkflow(Workflow workflow, StepPhase3Dto stepDto)
        {
            var step = workflow.Steps.FirstOrDefault(s => s.Id == stepDto.Id || s.Order == stepDto.Order);
            if (step == null)
            {
                throw new AppException(ErrorCode.NotFound, $"Step with order {stepDto.Order} not found",
                    StepLabel.NotFound);
            }

            return step;
        }

        /// <summary>
        /// Clears existing step tools and their dependencies.
        /// </summary>
        private async Task ClearExistingStepTools(Step step)
        {
            var stepToolIdsToRemove = step.StepTools.Select(st => st.Id).ToList();
            if (stepToolIdsToRemove.Any())
            {
                var hasOutputs = await _stepToolOutputRepository.HasOutputsByStepToolIds(stepToolIdsToRemove);
                if (hasOutputs)
                {
                    throw new AppException(
                        ErrorCode.ExistingStepToolOutput,
                        "Cannot delete step tools that have been executed and contain output data. Please remove the execution data first or create a new workflow version.",
                        null
                    );
                }

                await _stepToolDependencyRepository.DeleteByStepToolIdAsync(stepToolIdsToRemove);
            }

            step.StepTools.Clear();
        }

        /// <summary>
        /// Creates and configures a step tool with its dependencies.
        /// </summary>
        private StepTool CreateAndConfigureStepTool(
            StepToolUpdateDto stepToolDto,
            Step step,
            StepTool? previousStepToolInStep,
            StepTool? lastGlobalStepTool)
        {
            var stepTool = CreateStepToolUpdate(stepToolDto);
            stepTool.Step = step;
            stepTool.DependsOnStepTool = previousStepToolInStep ?? lastGlobalStepTool;
            return stepTool;
        }

        /// <summary>
        /// Creates dependencies for a step tool based on the DTO.
        /// </summary>
        private async Task CreateDependenciesForStepTool(
            Workflow workflow,
            StepTool stepTool,
            StepToolUpdateDto stepToolDto)
        {
            await _stepToolDependencyRepository.DeleteByStepToolIdAsync([stepTool.Id]);

            if (stepToolDto.Dependencies != null && stepToolDto.Dependencies.Count > 0)
            {
                foreach (var dependsOn in stepToolDto.Dependencies)
                {
                    var dependsOnStepTool = workflow.Steps
                        .SelectMany(s => s.StepTools)
                        .FirstOrDefault(st =>
                            st.Step!.Order == dependsOn.StepOrder && st.Order == dependsOn.StepToolOrder);

                    if (dependsOnStepTool != null && dependsOnStepTool.Id > 0)
                    {
                        var dependency = new StepToolDependency(0, DateTime.UtcNow, stepTool.Id, dependsOnStepTool.Id);
                        await _stepToolDependencyRepository.CreateAsync(dependency);
                    }
                }
            }
        }

        /// <summary>
        /// Find steps by workflow id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="workflowFilterDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<ICollection<StepDto>> FindStepsById(int id, WorkflowFilterDto? workflowFilterDto)
        {
            var input = workflowFilterDto?.Input ?? string.Empty;
            var allUsers = workflowFilterDto?.IsAllUsers ?? false;
            var login = workflowFilterDto?.Login ?? string.Empty;
            var order = workflowFilterDto?.OrderBy ?? string.Empty;

            var workflow = await _stepRepository.FindStepsByWorkflowId(id, input, allUsers, login, order);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }
            return workflow;
        }
    }
}
