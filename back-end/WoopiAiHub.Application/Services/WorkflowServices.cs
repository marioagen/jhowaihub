using System.Text.Json;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class WorkflowServices : IWorkflowServices
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IStepRepository _stepRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IStepToolDependencyRepository _stepToolDependencyRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IStepToolParameterRepository _stepToolParameterRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IToolRepository _toolRepository;
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IEncryptionService _encryptationService;
        private readonly ILogger<WorkflowServices> _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentDeletionServices _documentDeletionServices;
        private readonly IAuditCardService _auditCardService;
        private const string NotFoundMessage = "Workflow not found";
        private const int WorkflowDescriptionMaxLength = 500;

        public WorkflowServices(
            IWorkflowRepository workflowRepository,
            IProfileRepository profileRepository,
            ITeamRepository teamRepository,
            IStatusRepository statusRepository,
            IStepRepository stepRepository,
            ICardRepository cardRepository,
            IStepToolRepository stepToolRepository,
            IStepToolDependencyRepository stepToolDependencyRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            IStepToolExecutionRepository stepToolExecutionRepository,
            IStepToolParameterRepository stepToolParameterRepository,
            IUnitOfWork unitOfWork,
            IToolRepository toolRepository,
            IEncryptionService encryptationService,
            IDocumentRepository documentRepository,
            IDocumentDeletionServices documentDeletionServices,
            IAuditCardService auditCardService,
            ILogger<WorkflowServices> logger 
        )
        {
            _workflowRepository = workflowRepository;
            _profileRepository = profileRepository;
            _statusRepository = statusRepository;
            _stepRepository = stepRepository;
            _cardRepository = cardRepository;
            _stepToolRepository = stepToolRepository;
            _stepToolDependencyRepository = stepToolDependencyRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _stepToolParameterRepository = stepToolParameterRepository;
            _unitOfWork = unitOfWork;
            _teamRepository = teamRepository;
            _toolRepository = toolRepository;
            _encryptationService = encryptationService;
            _documentRepository = documentRepository;
            _documentDeletionServices = documentDeletionServices;
            _auditCardService = auditCardService;
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
        /// Documents that are exclusively linked to this workflow are also deleted to avoid orphans.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> DeleteById(int id, HeadersDto headersDto)
        {
            await HandleOrphanDocumentsAsync(id, headersDto);
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
                var paginatedList = PaginationHelper.Paginate(workflowList, workflowPagedDto.Page);
                return paginatedList;
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
        private async Task<StepTool> CreateStepToolUpdate(StepToolUpdateDto stepToolDto)
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

            var tool = await _toolRepository.FindModelByIdAsync(stepToolDto.ToolId)  ?? throw new AppException(ErrorCode.NotFound, "Tool not found", ToolLabel.NotFound);
            if (tool.ToolType?.Name == HandlersTypes.API)
            {
                foreach (var parameter in stepToolDto.Parameters)
                {
                    if (string.IsNullOrEmpty(parameter.Value))
                    {
                        continue;
                    }

                    var requiredFile = parameter.RequiredFile ?? false;
                    string paramValue = parameter.Value;

                    paramValue = NormalizeBodyToString(paramValue);

                    if (!_encryptationService.IsEncrypted(paramValue))
                    {
                        paramValue = _encryptationService.Encrypt(paramValue);
                    }

                    stepTool.Parameters.Add(
                        new StepToolParameter(0, DateTime.Now, 0, requiredFile, parameter.WebhookId, paramValue));
                }
            }
            else
            {
                foreach (var parameter in stepToolDto.Parameters)
                {
                    var requiredFile = parameter.RequiredFile ?? false;
                    stepTool.Parameters.Add(
                        new StepToolParameter(0, DateTime.Now, 0, requiredFile, parameter.WebhookId,
                            parameter.Value));
                }
            }

            return stepTool;
        }

        /// <summary>
        /// Normalizes the body property in an API request JSON to ensure it is stored as a string.
        /// If the body is a JSON object, it will be serialized to a JSON string.
        /// </summary>
        /// <param name="jsonValue">The JSON string containing the API request configuration.</param>
        /// <returns>The normalized JSON string with the body as a string value.</returns>
        private static string NormalizeBodyToString(string jsonValue)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                using var document = JsonDocument.Parse(jsonValue);
                var root = document.RootElement;

                if (!root.TryGetProperty("body", out var bodyProperty))
                {
                    return jsonValue;
                }

                if (bodyProperty.ValueKind == JsonValueKind.String)
                {
                    return jsonValue;
                }

                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions 
                { 
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                });

                writer.WriteStartObject();

                foreach (var property in root.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);

                    if (property.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                    {
                        var bodyJson = JsonSerializer.Serialize(property.Value, options);
                        writer.WriteStringValue(bodyJson);
                    }
                    else
                    {
                        property.Value.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
                writer.Flush();

                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                return jsonValue;
            }
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
            var stepToolOutput = this.FindByStepToolOutputById(outputUpdateDto.Id)
                ?? throw new AppException(ErrorCode.NotFound, "Step tool output not found", null);
            stepToolOutput.ChangeValue(outputUpdateDto.Value);
            var result = await _workflowRepository.UpdateStepToolOutput(stepToolOutput);
            return result;
        }

        /// <summary>
        ///  Find StepToolOutput by step id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StepToolOutput? FindByStepToolOutputById(int id)
        {
            return _workflowRepository.FindByStepToolOutputById(id);
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

            if (workflowPhase1Dto.Description.Length > WorkflowDescriptionMaxLength)
            {
                throw new AppException(ErrorCode.InvalidValue,
                    $"Workflow description cannot exceed {WorkflowDescriptionMaxLength} characters",
                    WorkflowLabel.InvalidDescription);
            }

            var workflow = new Workflow(0, DateTime.UtcNow, teamsList, workflowPhase1Dto.Name, workflowPhase1Dto.Description);
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
            var step = _workflowRepository.FindStepById(id)
                ?? throw new AppException(ErrorCode.NotFound, "Step not found", StepLabel.NotFound);

            var apiTools = step.StepTools.Where(w => w.Tool?.ToolType == HandlersTypes.API).ToList();
            if (apiTools is not null && apiTools.Count > 0)
            {
                foreach(var apiTool in apiTools)
                {
                    foreach(var apiToolParam in apiTool.Parameters)
                    {
                        apiToolParam.Value = _encryptationService.Decrypt(apiToolParam.Value);
                    }
                }
            }

            return step;
        }

        /// <summary>
        /// Retrieves a list of workflows by documentId and user .
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ICollection<ResponseWorkflowByDocumentDto>> FindWorkflowsByDocument(RequestWorkFlowByDocumentDto dto, CancellationToken ct = default)
        {
            return await _workflowRepository.FindWorkflowsByDocument(dto, ct);
        }

        /// <summary>
        /// Phase 2: Updates a workflow with steps information (without step tools).
        /// Validates and creates/updates steps with their profiles and statuses.
        /// Documents exclusively linked to removed steps and this workflow are deleted to avoid orphans.
        /// </summary>
        /// <param name="workflowPhase2Dto"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePhase2(WorkflowPhase2Dto workflowPhase2Dto, HeadersDto headersDto)
        {
            var workflow = await _workflowRepository.FindByIdReturnModel(workflowPhase2Dto.WorkflowId);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            var existingSteps = workflow.Steps.ToList();

            var newStepsDict = workflowPhase2Dto.Steps
                .Where(s => s.Id > 0)
                .ToDictionary(s => s.Id);

            var stepsToRemove = existingSteps
                .Where(es => !newStepsDict.ContainsKey(es.Id))
                .ToList();

            var auditRemovedPairs = new List<(int cardId, int documentId)>();
            var orphanCandidatePairs = new List<(int cardId, int documentId)>();
            if (stepsToRemove.Count > 0 && workflowPhase2Dto.ResetDocuments)
            {
                var minRemovedOrder = stepsToRemove.Min(s => s.Order);
                var allResetStepIds = workflow.Steps
                    .Where(s => s.Order >= minRemovedOrder)
                    .Select(s => s.Id).ToList();
                auditRemovedPairs = await _cardRepository.FindCardDocumentPairsByStepIdsAsync(allResetStepIds);
                orphanCandidatePairs = await _cardRepository
                    .FindCardDocumentPairsByStepIdsAsync(stepsToRemove.Select(s => s.Id).ToList());
            }

            var stepsToUpdate = existingSteps
                .Where(es => newStepsDict.ContainsKey(es.Id))
                .ToList();

            var stepsToAdd = workflowPhase2Dto.Steps
                .Where(s => s.Id == 0 || !existingSteps.Any(es => es.Id == s.Id))
                .ToList();

            _unitOfWork.BeginTransaction();
            try
            {
                await ResetStepToolDataAsync(workflow, workflowPhase2Dto.ResetDocuments, stepsToRemove);

                if (stepsToRemove.Count > 0)
                {
                    var cardsCount = await _cardRepository.CountByStepsInUse(stepsToRemove.Select(s => s.Id).ToList());
                    if (cardsCount > 0)
                    {
                        throw new AppException(ErrorCode.DefaultError, "Can't delete with cards related", null);
                    }
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

                if (auditRemovedPairs.Count > 0)
                {
                    var removedTuples = auditRemovedPairs
                        .Select(p => (p.cardId, workflowPhase2Dto.WorkflowId, p.documentId))
                        .ToList<(int, int, int)>();
                    await _auditCardService.CreateBatchAndSaveAsync(removedTuples, AuditCardActionType.Removed);
                }

                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }

            await HandleOrphanDocumentsWithAuditAsync(
                workflowPhase2Dto.WorkflowId, headersDto, orphanCandidatePairs);

            return true;
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

            if (workflowUpdatePhase1Dto.Description.Length > WorkflowDescriptionMaxLength)
            {
                throw new AppException(ErrorCode.InvalidValue,
                    $"Workflow description cannot exceed {WorkflowDescriptionMaxLength} characters",
                    WorkflowLabel.InvalidDescription);
            }

            _unitOfWork.BeginTransaction();
            workflow.Teams.Clear();
            try
            {
                var teamsList = _teamRepository.FindByIds(workflowUpdatePhase1Dto.Teams);
                foreach (var team in teamsList)
                {
                    workflow.AddTeam(team);
                }

                workflow.Update(workflowUpdatePhase1Dto.Name, workflowUpdatePhase1Dto.Description);
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
        /// When <paramref name="resetDocuments"/> is <see langword="true"/> and there are removed steps, removes all transactional data
        /// (StepToolDependency, StepToolOutput, StepToolExecution, StepToolParameter) for StepTools
        /// belonging to steps that have an Order greater than or equal to the minimum Order of the removed steps.
        /// It also sets Enable = false for all Cards in these steps.
        /// This prevents referential integrity exceptions when StepTools are removed or reordered.
        /// </summary>
        /// <param name="workflow">The workflow model with Steps and StepTools loaded.</param>
        /// <param name="resetDocuments">When false, the method returns immediately without any changes.</param>
        /// <param name="stepsToRemove">The list of steps that are being removed in this phase update.</param>
        private async Task ResetStepToolDataAsync(Workflow workflow, bool resetDocuments, List<Step> stepsToRemove)
        {
            if (!resetDocuments || stepsToRemove.Count == 0)
                return;

            var minRemovedOrder = stepsToRemove.Min(s => s.Order);
            var stepsToReset = workflow.Steps.Where(s => s.Order >= minRemovedOrder).ToList();

            await ResetSteps(stepsToReset);
        }

        /// <summary>
        /// Resets the specified steps by removing all related transactional data and associated entities.
        /// </summary>
        /// <remarks>This method deletes dependent data in the correct order to maintain referential
        /// integrity. All related executions, outputs, parameters, dependencies, and cards associated with the provided
        /// steps are removed. The operation is asynchronous and should be awaited to ensure completion.</remarks>
        /// <param name="stepsToReset">The list of steps to reset. Each step and its related data will be deleted or cleared as part of the reset
        /// operation. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous reset operation.</returns>
        private async Task ResetSteps(List<Step> stepsToReset)
        {
            var allStepToolIds = new List<int>();
            var allCardIds = new List<int>();

            foreach (var step in stepsToReset)
            {
                if (step.Cards != null && step.Cards.Count > 0)
                {
                    allCardIds.AddRange(step.Cards.Select(c => c.Id));
                }

                var stepToolIds = step.StepTools.Select(st => st.Id).ToList();
                if (stepToolIds.Count > 0)
                {
                    allStepToolIds.AddRange(stepToolIds);
                }
            }

            await DeleteRelatedStepData(allStepToolIds, allCardIds);
        }

        /// <summary>
        /// Deletes data associated with the specified step tool and card identifiers.
        /// </summary>
        /// <param name="allStepToolIds">A list of step tool identifiers for which related data will be deleted. Cannot be null.</param>
        /// <param name="allCardIds">A list of card identifiers for which related step data will be deleted. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        private async Task DeleteRelatedStepData(List<int> allStepToolIds, List<int> allCardIds)
        {
            await DeleteStepToolRelatedData(allStepToolIds);

            await DeleteRelatedStepsCardData(allCardIds);
        }

        /// <summary>
        /// Deletes all step tool execution, step tool output, audit card, and card data associated with the specified
        /// card IDs.
        /// </summary>
        /// <remarks>This method removes data from multiple repositories based on the provided card IDs.
        /// If the list is empty, no action is taken.</remarks>
        /// <param name="allCardIds">A list of card IDs for which related step and card data will be deleted. Must not be null; if empty, no data
        /// will be deleted.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        private async Task DeleteRelatedStepsCardData(List<int> allCardIds)
        {
            if (allCardIds.Count > 0)
            {
                _stepToolExecutionRepository.DeleteByCardIds(allCardIds);
                _stepToolOutputRepository.DeleteByCardIds(allCardIds);
                await _cardRepository.DisableByIds(allCardIds);
            }
        }

        /// <summary>
        /// Deletes all data related to the specified step tool identifiers, including parameters, dependencies,
        /// executions, and outputs.
        /// </summary>
        /// <remarks>This method removes all associated data for each provided step tool identifier. If
        /// the list is empty, no action is taken.</remarks>
        /// <param name="allStepToolIds">A list of step tool identifiers for which related data will be deleted. The list must not be empty.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        private async Task DeleteStepToolRelatedData(List<int> allStepToolIds)
        {
            if (allStepToolIds.Count > 0)
            {
                _stepToolParameterRepository.DeleteByStepToolsIds(allStepToolIds);
                await _stepToolDependencyRepository.DeleteByStepToolIdAsync(allStepToolIds);
                await _stepToolExecutionRepository.DeleteByStepToolIdsAsync(allStepToolIds);
                await _stepToolOutputRepository.DeleteByStepToolIdsAsync(allStepToolIds);
            }
        }

        /// <summary>
        /// Detects documents that are exclusively linked to the given workflow (orphans) and deletes them
        /// via <see cref="IDocumentDeletionServices"/>. Used in Scenario 1 (DeleteById) where cards are
        /// still active — the deletion service handles audit internally.
        /// </summary>
        private async Task HandleOrphanDocumentsAsync(int workflowId, HeadersDto headersDto,
            List<int>? candidateDocumentIds = null)
        {
            var orphanIds = await _documentRepository
                .FindOrphanDocumentIdsByWorkflowAsync(workflowId, candidateDocumentIds);
            if (orphanIds.Count > 0)
                await _documentDeletionServices.Delete(orphanIds, headersDto);
        }

        /// <summary>
        /// Detects orphan documents from <paramref name="cardDocumentPairs"/>, creates the
        /// <see cref="AuditCardActionType.DocumentDeleted"/> audit events (cards are already disabled at
        /// this point so the deletion service's internal audit would be a no-op), then physically deletes
        /// the orphan documents. Used in Scenarios 2 and 3 where cards are disabled before this runs.
        /// </summary>
        private async Task HandleOrphanDocumentsWithAuditAsync(
            int workflowId,
            HeadersDto headersDto,
            IReadOnlyList<(int cardId, int documentId)> cardDocumentPairs)
        {
            if (!cardDocumentPairs.Any()) return;

            var candidateDocumentIds = cardDocumentPairs.Select(p => p.documentId).Distinct().ToList();
            var orphanIds = await _documentRepository
                .FindOrphanDocumentIdsByWorkflowAsync(workflowId, candidateDocumentIds);
            if (orphanIds.Count == 0) return;

            var documentDeletedTuples = cardDocumentPairs
                .Where(p => orphanIds.Contains(p.documentId))
                .GroupBy(p => p.documentId)
                .Select(g => (g.First().cardId, workflowId, g.Key))
                .ToList<(int, int, int)>();

            if (documentDeletedTuples.Count > 0)
                await _auditCardService.CreateBatchAndSaveAsync(documentDeletedTuples, AuditCardActionType.DocumentDeleted);

            await _documentDeletionServices.Delete(orphanIds, headersDto);
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
        /// Documents exclusively linked to this workflow whose cards are disabled by the reset are deleted to avoid orphans.
        /// </summary>
        /// <param name="workflowPhase3Dto"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePhase3(WorkflowPhase3Dto workflowPhase3Dto, HeadersDto headersDto)
        {
            var workflow = await _workflowRepository.FindByIdForFlow(workflowPhase3Dto.WorkflowId);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            var cardDocumentPairs = new List<(int cardId, int documentId)>();
            if (workflowPhase3Dto.ResetDocuments)
            {
                var affectedStepIds = workflow.Steps.Select(s => s.Id).ToList();
                cardDocumentPairs = await _cardRepository.FindCardDocumentPairsByStepIdsAsync(affectedStepIds);
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var stepToolMap = await ProcessStepTools(workflow, workflowPhase3Dto.Steps, workflowPhase3Dto.ResetDocuments);
                await ResolveDependencies(workflow, workflowPhase3Dto.Steps, stepToolMap);

                if (cardDocumentPairs.Count > 0)
                {
                    var removedTuples = cardDocumentPairs
                        .Select(p => (p.cardId, workflowPhase3Dto.WorkflowId, p.documentId))
                        .ToList<(int, int, int)>();
                    await _auditCardService.CreateBatchAndSaveAsync(removedTuples, AuditCardActionType.Removed);
                }

                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }

            await HandleOrphanDocumentsWithAuditAsync(
                workflowPhase3Dto.WorkflowId, headersDto, cardDocumentPairs);

            return true;
        }

        /// <summary>
        /// Processes step tools for each step in the workflow.
        /// </summary>
        private async Task<Dictionary<(int stepId, int order), StepTool>> ProcessStepTools(
            Workflow workflow,
            ICollection<StepPhase3Dto> steps,
            bool resetDocuments)
        {
            StepTool? lastGlobalStepTool = null;
            var stepToolMap = new Dictionary<(int stepId, int order), StepTool>();

            foreach (var stepDto in steps.OrderBy(s => s.Order))
            {
                var existingStep = FindStepInWorkflow(workflow, stepDto);
                await ClearExistingStepTools(existingStep, resetDocuments);

                StepTool? previousStepToolInStep = null;

                foreach (var stepToolDto in stepDto.StepTools.OrderBy(st => st.Order))
                {
                    var stepTool = await CreateAndConfigureStepTool(
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
        private async Task ClearExistingStepTools(Step step, bool resetDocuments)
        {
            var stepToolIdsToRemove = step.StepTools.Select(st => st.Id).ToList();
            if (resetDocuments)
            {
                await DeleteStepToolRelatedData(stepToolIdsToRemove);
                var stepWithCards = await _stepRepository.FindById(step.Id);
                if (stepWithCards != null)
                {
                    await DeleteRelatedStepsCardData(stepWithCards.Cards.Select(c => c.Id).ToList());
                }
            }
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
        private async Task<StepTool> CreateAndConfigureStepTool(
            StepToolUpdateDto stepToolDto,
            Step step,
            StepTool? previousStepToolInStep,
            StepTool? lastGlobalStepTool)
        {
            var stepTool = await CreateStepToolUpdate(stepToolDto);
            stepTool.Step = step;
            stepTool.DependsOnStepTool = previousStepToolInStep ?? lastGlobalStepTool;
            return stepTool;
        }

        /// <summary>
        /// Creates dependencies for a step tool based on the DTO.
        /// Validates Prompt tool dependency rules when applicable.
        /// </summary>
        private async Task CreateDependenciesForStepTool(Workflow workflow, StepTool stepTool, StepToolUpdateDto stepToolDto)
        {
            await _stepToolDependencyRepository.DeleteByStepToolIdAsync([stepTool.Id]);

            var tool = await _toolRepository.FindModelByIdAsync(stepTool.ToolId) ?? throw new AppException(ErrorCode.NotFound, "Tool not found", ToolLabel.NotFound);

            var createdDependencies = await FindStepToolDependencies(workflow, stepTool, stepToolDto);

            await ValidatePromptTool(tool, createdDependencies, stepToolDto);
            await ValidateQuizTool(tool, createdDependencies, stepToolDto);
        }

        /// <summary>
        /// Finds and creates dependencies between the specified step tool and other step tools within the given
        /// workflow, based on the provided dependency information.
        /// </summary>
        /// <remarks>This method creates dependency records for each valid dependency specified in
        /// <paramref name="stepToolDto"/>. Only dependencies that correspond to existing step tools in the workflow are
        /// created and returned.</remarks>
        /// <param name="workflow">The workflow containing the steps and step tools to search for dependencies.</param>
        /// <param name="stepTool">The step tool for which dependencies are being established.</param>
        /// <param name="stepToolDto">The data transfer object containing dependency information for the step tool. Must not be <c>null</c>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="StepTool"/>
        /// instances that were found and linked as dependencies. The list is empty if no dependencies were specified or
        /// found.</returns>
        private async Task<List<StepTool>> FindStepToolDependencies(Workflow workflow, StepTool stepTool, StepToolUpdateDto stepToolDto)
        {
            var createdDependencies = new List<StepTool>();

            if (stepToolDto.Dependencies == null || stepToolDto.Dependencies.Count == 0)
            {
                return createdDependencies;
            }

            foreach (var dependsOn in stepToolDto.Dependencies)
            {
                var dependsOnStepTool = workflow.Steps
                    .Where(s => s.Order == dependsOn.StepOrder)
                    .SelectMany(s => s.StepTools)
                    .FirstOrDefault(st => st.Order == dependsOn.StepToolOrder);

                if (dependsOnStepTool != null)
                {
                    var dependency = new StepToolDependency(0, DateTime.UtcNow, stepTool.Id, dependsOnStepTool.Id);
                    await _stepToolDependencyRepository.CreateAsync(dependency);
                    createdDependencies.Add(dependsOnStepTool);
                }
            }

            return createdDependencies;
        }

        /// <summary>
        /// Ensures a Prompt tool lists at least one dependency and that every linked dependency is an API or Quiz tool.
        /// </summary>
        /// <param name="tool">Tool metadata for the step tool being saved.</param>
        /// <param name="createdDependencies">Dependencies resolved from the workflow and DTO.</param>
        /// <param name="stepToolDto">Incoming step tool payload.</param>
        private async Task ValidatePromptTool(Tool tool, List<StepTool> createdDependencies, StepToolUpdateDto stepToolDto)
        {
            if (tool.ToolType?.Name != HandlersTypes.Prompt)
            {
                return;
            }

            if (stepToolDto.Dependencies == null || stepToolDto.Dependencies.Count == 0 || createdDependencies.Count == 0)
            {
                throw new AppException(ErrorCode.RequiredField, "Prompt tool must have at least one dependency", ToolLabel.DependecyRequired);
            }

            var toolCache = new Dictionary<int, Tool> { { tool.Id, tool } };
            await EnsurePromptDependenciesAreApiOrQuizOnly(createdDependencies, toolCache);
        }

        /// <summary>
        /// Ensures a Quiz tool lists at least one dependency and that at least one is an Embeddings tool.
        /// </summary>
        /// <param name="tool">Tool metadata for the step tool being saved.</param>
        /// <param name="createdDependencies">Dependencies resolved from the workflow and DTO.</param>
        /// <param name="stepToolDto">Incoming step tool payload.</param>
        private async Task ValidateQuizTool(Tool tool, List<StepTool> createdDependencies, StepToolUpdateDto stepToolDto)
        {
            if (tool.ToolType?.Name != HandlersTypes.Quiz)
            {
                return;
            }

            if (stepToolDto.Dependencies == null || stepToolDto.Dependencies.Count == 0 || createdDependencies.Count == 0)
            {
                throw new AppException(ErrorCode.RequiredField, "Quiz tool must have at least one dependency", ToolLabel.DependecyRequired);
            }

            var toolCache = new Dictionary<int, Tool> { { tool.Id, tool } };
            var hasEmbeddingDependency = await HasEmbeddingDependency(createdDependencies, toolCache);
            if (!hasEmbeddingDependency)
            {
                throw new AppException(ErrorCode.RequiredField, "Quiz tool must have at least one Embedding dependency", ToolLabel.EmbeddingDependencyRequired);
            }
        }

        /// <summary>
        /// Throws when any Prompt dependency is missing tool metadata or is not an API or Quiz tool.
        /// </summary>
        /// <param name="dependencies">Resolved dependency step tools.</param>
        /// <param name="toolCache">Cache of tool id to tool model, updated during resolution.</param>
        private async Task EnsurePromptDependenciesAreApiOrQuizOnly(List<StepTool> dependencies, Dictionary<int, Tool> toolCache)
        {
            foreach (var dependency in dependencies)
            {
                if (!toolCache.TryGetValue(dependency.ToolId, out var dependencyTool))
                {
                    dependencyTool = await _toolRepository.FindModelByIdAsync(dependency.ToolId);
                    if (dependencyTool != null)
                    {
                        toolCache[dependency.ToolId] = dependencyTool;
                    }
                }

                if (dependencyTool == null)
                {
                    throw new AppException(ErrorCode.NotFound, "Dependency tool not found", ToolLabel.DependencyToolNotFound);
                }

                var typeName = dependencyTool.ToolType?.Name;
                if (typeName != HandlersTypes.API
                    && typeName != HandlersTypes.Quiz
                    && typeName != HandlersTypes.Ocr
                    && typeName != HandlersTypes.Parser
                    && typeName!= HandlersTypes.Prompt)
                {
                    throw new AppException(
                        ErrorCode.RequiredField,
                        "Prompt tool dependencies must be API, Quiz, OCR or Prompt tools only",
                        ToolLabel.PromptApiOrQuizDependencyRequired);
                }
            }
        }

        /// <summary>
        /// Determines whether any of the specified dependencies require an Embedding tool.
        /// </summary>
        /// <remarks>This method checks each dependency to determine if it is associated with a tool of
        /// type Embedding. The <paramref name="toolCache"/> is used to avoid redundant lookups and may be populated with
        /// additional tools as needed.</remarks>
        /// <param name="dependencies">A list of <see cref="StepTool"/> objects representing the tool dependencies to check.</param>
        /// <param name="toolCache">A dictionary that maps tool IDs to <see cref="Tool"/> instances, used to cache tool lookups and improve
        /// performance. May be updated with additional entries during execution.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if at least one
        /// dependency requires an embedding tool; otherwise, <see langword="false"/>.</returns>
        private async Task<bool> HasEmbeddingDependency(List<StepTool> dependencies, Dictionary<int, Tool> toolCache)
        {
            foreach (var dependency in dependencies)
            {
                if (!toolCache.TryGetValue(dependency.ToolId, out var dependencyTool))
                {
                    dependencyTool = await _toolRepository.FindModelByIdAsync(dependency.ToolId);
                    if (dependencyTool != null)
                    {
                        toolCache[dependency.ToolId] = dependencyTool;
                    }
                }

                if (dependencyTool?.ToolType?.Name == HandlersTypes.Embeddings  )
                {
                    return true;
                }
            }

            return false;
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
            var documentFilter = workflowFilterDto?.Document ?? DocumentFilter.All;

            return await _stepRepository.FindStepsByWorkflowId(id, input, allUsers, login, order, documentFilter) ?? throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
        }

        /// <summary>
        /// Creates a deep copy of an existing workflow with a new name.
        /// Copies steps, step tools, parameters, dependencies and team associations.
        /// Does not copy documents. The source workflow is not modified.
        /// </summary>
        public async Task<int> CloneAsync(WorkflowCloneRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewName))
            {
                throw new AppException(ErrorCode.RequiredField, "Workflow name is required", WorkflowLabel.InvalidName);
            }

            var source = await _workflowRepository.FindByIdForClone(dto.SourceWorkflowId);
            if (source == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            var teamIds = source.Teams.Select(t => t.Id).ToList();
            var teamsList = _teamRepository.FindByIds(teamIds);
            if (teamsList.Count != teamIds.Count)
            {
                throw new AppException(ErrorCode.NotFound, "One or more teams not found", TeamLabel.NotFound);
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var newWorkflow = new Workflow(0, DateTime.UtcNow, teamsList, dto.NewName, source.Description);
                await _workflowRepository.Create(newWorkflow);

                var sourceStepsOrdered = source.Steps.OrderBy(s => s.Order).ToList();
                AddClonedSteps(newWorkflow, sourceStepsOrdered);
                await _workflowRepository.Update(newWorkflow);

                var (newStepToolsList, sourceStepToolsList) = AddClonedStepTools(newWorkflow, sourceStepsOrdered);
                await _stepToolRepository.CreateRangeAsync(newStepToolsList);

                ApplyClonedDependencies(newStepToolsList, sourceStepToolsList);
                await _unitOfWork.SaveChangesAsync();

                await CreateClonedStepToolDependencies(newStepToolsList, sourceStepToolsList);

                _unitOfWork.Commit();
                return newWorkflow.Id;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Adds cloned steps to the new workflow.
        /// </summary>
        /// <param name="newWorkflow">The new workflow instance.</param>
        /// <param name="sourceStepsOrdered">The ordered list of steps from the source workflow.</param>
        private static void AddClonedSteps(Workflow newWorkflow, List<Step> sourceStepsOrdered)
        {
            foreach (var sourceStep in sourceStepsOrdered)
            {
                var newStep = new Step(
                    0,
                    DateTime.UtcNow,
                    newWorkflow.Id,
                    sourceStep.Name,
                    sourceStep.Order,
                    sourceStep.ProfileId,
                    sourceStep.StatusId);
                newWorkflow.AddStep(newStep);
            }
        }

        /// <summary>
        /// Adds cloned step tools to the new workflow steps.
        /// </summary>
        /// <param name="newWorkflow">The new workflow instance.</param>
        /// <param name="sourceStepsOrdered">The ordered list of steps from the source workflow.</param>
        /// <returns>A tuple containing lists of new and source step tools for dependency mapping.</returns>
        private static (List<StepTool> NewStepTools, List<StepTool> SourceStepTools) AddClonedStepTools(
            Workflow newWorkflow,
            List<Step> sourceStepsOrdered)
        {
            var newStepsOrdered = newWorkflow.Steps.OrderBy(s => s.Order).ToList();
            var newStepToolsList = new List<StepTool>();
            var sourceStepToolsList = new List<StepTool>();

            for (var i = 0; i < sourceStepsOrdered.Count; i++)
            {
                var sourceStep = sourceStepsOrdered[i];
                var newStep = newStepsOrdered[i];

                foreach (var sourceStepTool in sourceStep.StepTools.OrderBy(st => st.Order))
                {
                    var newStepTool = CreateClonedStepTool(newStep.Id, sourceStepTool);
                    newStep.AddStepTool(newStepTool);
                    newStepToolsList.Add(newStepTool);
                    sourceStepToolsList.Add(sourceStepTool);
                }
            }

            return (newStepToolsList, sourceStepToolsList);
        }

        /// <summary>
        /// Creates a deep copy of a step tool including its parameters.
        /// </summary>
        /// <param name="newStepId">The ID of the new step.</param>
        /// <param name="sourceStepTool">The source step tool to clone.</param>
        /// <returns>The new cloned StepTool instance.</returns>
        private static StepTool CreateClonedStepTool(int newStepId, StepTool sourceStepTool)
        {
            var newStepTool = new StepTool(
                0,
                DateTime.UtcNow,
                newStepId,
                sourceStepTool.ToolId,
                sourceStepTool.Order,
                sourceStepTool.PositionX,
                sourceStepTool.PositionY);

            foreach (var param in sourceStepTool.Parameters)
            {
                var newParam = new StepToolParameter(
                    0,
                    DateTime.UtcNow,
                    0,
                    param.RequiredFile,
                    param.WebhookId,
                    param.Value);
                newStepTool.Parameters.Add(newParam);
            }

            return newStepTool;
        }

        /// <summary>
        /// Updates the in-memory dependencies (linked list) for the cloned step tools.
        /// </summary>
        /// <param name="newStepToolsList">List of new step tools.</param>
        /// <param name="sourceStepToolsList">List of source step tools corresponding directly to the new list.</param>
        private void ApplyClonedDependencies(List<StepTool> newStepToolsList, List<StepTool> sourceStepToolsList)
        {
            var sourceIdToIndex = new Dictionary<int, int>();
            for (var i = 0; i < sourceStepToolsList.Count; i++)
            {
                sourceIdToIndex[sourceStepToolsList[i].Id] = i;
            }

            for (var i = 0; i < sourceStepToolsList.Count; i++)
            {
                var sourceStepTool = sourceStepToolsList[i];
                var newStepTool = newStepToolsList[i];

                if (sourceStepTool.DependsOnStepToolId is { } dependsOnId &&
                    sourceIdToIndex.TryGetValue(dependsOnId, out var depIndex))
                {
                    newStepTool.UpdateDependencyStepTool(newStepToolsList[depIndex]);
                }
            }
        }

        /// <summary>
        /// Creates the database dependency records (StepToolDependency) for the cloned step tools.
        /// </summary>
        /// <param name="newStepToolsList">List of new step tools.</param>
        /// <param name="sourceStepToolsList">List of source step tools corresponding directly to the new list.</param>
        private async Task CreateClonedStepToolDependencies(
            List<StepTool> newStepToolsList,
            List<StepTool> sourceStepToolsList)
        {
            var sourceIdToIndex = new Dictionary<int, int>();
            for (var i = 0; i < sourceStepToolsList.Count; i++)
            {
                sourceIdToIndex[sourceStepToolsList[i].Id] = i;
            }

            var newDependencies = new List<StepToolDependency>();
            for (var i = 0; i < sourceStepToolsList.Count; i++)
            {
                var sourceStepTool = sourceStepToolsList[i];
                var newStepTool = newStepToolsList[i];

                foreach (var dep in sourceStepTool.Dependencies)
                {
                    if (sourceIdToIndex.TryGetValue(dep.DependsOnStepToolId, out var depIndex))
                    {
                        newDependencies.Add(new StepToolDependency(
                            0,
                            DateTime.UtcNow,
                            newStepTool.Id,
                            newStepToolsList[depIndex].Id));
                    }
                }
            }

            if (newDependencies.Count > 0)
            {
                await _stepToolDependencyRepository.CreateRangeAsync(newDependencies);
            }
        }

        /// <summary>
        /// Returns all workflows in a simplified format for internal
        /// </summary>
        /// <returns></returns>
        public ICollection<WorkflowInternalDto> FindAllInternal()
        {
            return _workflowRepository.FindAllInternal();
        }

        /// <summary>
        /// Retrieves a workflow model by its ID, including its steps and associated data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Workflow?> FindModelById(int id)
        {
            return _workflowRepository.FindByIdReturnModelWithSteps(id);
        }

        /// <summary>
        /// Asynchronously counts every card associated with the steps of the specified workflow,
        /// including soft-deleted (disabled) ones. Used by the wizard UI to decide whether the
        /// blocker modal must appear before destructive edits like step removal — historical
        /// cards still hold FK references to the Step (Restrict), so allowing the deletion would
        /// fail at the database level. Blocking here also preserves audit traceability since no
        /// data needs to be hard-deleted.
        /// </summary>
        /// <param name="id">The unique identifier of the workflow for which to count associated cards.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of cards
        /// (active or disabled) linked to the workflow's steps.</returns>
        /// <exception cref="AppException">Thrown if a workflow with the specified identifier does not exist.</exception>
        public async Task<int> CountCards(int id)
        {
            var workflow = await FindWorkflowModel(id);
            var stepIds = workflow.Steps.Select(s => s.Id).ToList();
            return await _cardRepository.CountAllByStepIdsAsync(stepIds);
        }

        /// <summary>
        /// Checks whether the given Step has associated transactional data that would prevent
        /// the removal of its tool flow. Verifies StepToolOutput, StepToolExecution,
        /// StepToolDependency (as source or target) and linked Cards.
        /// </summary>
        /// <param name="stepId">The ID of the Step to check.</param>
        /// <returns>True if any constraint exists; otherwise, false.</returns>
        public async Task<bool> HasStepToolConstraints(int stepId)
        {
            var step = await _stepRepository.FindByIdWithTools(stepId);
            if (step == null)
                return false;

            var stepToolIds = step.StepTools.Select(st => st.Id).ToList();

            if (stepToolIds.Count > 0)
            {
                if (await _stepToolOutputRepository.HasOutputsByStepToolIds(stepToolIds))
                    return true;

                if (await _stepToolExecutionRepository.HasExecutionsByStepToolIdsAsync(stepToolIds))
                    return true;

                if (await _stepToolDependencyRepository.HasDependenciesByStepToolIdsAsync(stepToolIds))
                    return true;
            }

            var cardCount = await _cardRepository.CountByStepsInUse(new List<int> { stepId });
            return cardCount > 0;
        }

        /// <summary>
        /// Retrieves the workflow model with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the workflow to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the workflow model if found;
        /// otherwise, the method throws an exception.</returns>
        /// <exception cref="AppException">Thrown if a workflow with the specified identifier is not found.</exception>
        private async Task<Workflow> FindWorkflowModel(int id)
        {
            var workflow = await _workflowRepository.FindByIdReturnModel(id);
            if (workflow == null)
            {
                throw new AppException(ErrorCode.NotFound, NotFoundMessage, WorkflowLabel.NotFound);
            }

            return workflow;
        }
    }
}
