using System.Collections.Generic;
using Newtonsoft.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class CardServices : ICardServices
    {
        private readonly ICardRepository _cardRepository;
        private readonly IAuditCardService _auditCardService;
        private readonly IStepRepository _stepRepository;
        private readonly IAutomationServices _automationServices;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IPromptServices _promptServices;
        private readonly IQuestionnaireServices _questionnaireServices;

        private const string CardNotFoundMessage = "Card not found";

        /// <summary>
        /// Initializes a new instance of the <see cref="CardServices"/> class with card, workflow, audit, and automation dependencies.
        /// </summary>
        public CardServices(ICardRepository cardRepository,
                            IAuditCardService auditCardService,
                            IStepRepository stepRepository,
                            IAutomationServices automationServices,
                            IStepToolExecutionRepository stepToolExecutionRepository,
                            IWorkflowRepository workflowRepository,
                            IPromptServices promptServices,
                            IQuestionnaireServices questionnaireServices)
        {
            _cardRepository = cardRepository;
            _auditCardService = auditCardService;
            _stepRepository = stepRepository;
            _automationServices = automationServices;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _workflowRepository = workflowRepository;
            _promptServices = promptServices;
            _questionnaireServices = questionnaireServices;
        }

        /// <summary>
        /// Updates assigned user
        /// </summary>
        /// <param name="updateAssingnedUserDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> AssignUser(UpdateAssignedUserDto updateAssingnedUserDto)
        {
            if (updateAssingnedUserDto.UserId == Guid.Empty)
            {
                throw new ArgumentNullException(updateAssingnedUserDto.UserId.ToString(), "Invalid UserId");
            }

            var cards = await _cardRepository.FindCardOrBatchWithStepWorkflowAsync(updateAssingnedUserDto.CardId);
            if (cards == null || cards.Count == 0)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);
            }

            var isValidTeamUser = await _workflowRepository.IsValidTeamUser(updateAssingnedUserDto.CardId,
                                                                            updateAssingnedUserDto.UserId);

            if (!isValidTeamUser)
            {
                throw new AppException(ErrorCode.NotFound, "User not found",
                    CardLabel.UserCannotBeAssigned);
            }

            Card.UpdateAssignedUser(cards, updateAssingnedUserDto.UserId);

            var cardWorkflows = cards.Select(card => (card.Id, card.Step!.WorkflowId, card.DocumentId)).ToList();
            if (cardWorkflows.Count > 0)
            {
                await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Assign);
            }

            return await _cardRepository.UpdateList(cards);
        }

        /// <summary>
        /// Updates assigned user to null
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UnassignUser(int cardId)
        {
            var cards = await _cardRepository.FindCardOrBatchWithStepWorkflowAsync(cardId);
            if (cards == null || cards.Count == 0)
                throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);

            Card.UpdateAssignedUser(cards, null);

            var cardWorkflows = cards.Select(card => (card.Id, card.Step!.WorkflowId, card.DocumentId)).ToList();
            if (cardWorkflows.Count > 0)
            {
                await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Unassign);
            }

            return await _cardRepository.UpdateList(cards);
        }

        /// <summary>
        /// Assigns a user to all cards resolved from the distinct ids in <paramref name="request"/> in one operation.
        /// </summary>
        public async Task<bool> AssignRangeAsync(AssignRangeDto assignRangeDto)
        {
            ArgumentNullException.ThrowIfNull(assignRangeDto);
            if (assignRangeDto.CardIds == null || assignRangeDto.CardIds.Count == 0)
            {
                throw new ArgumentException("CardIds cannot be empty.", nameof(assignRangeDto));
            }

            var uniqueCardIds = assignRangeDto.CardIds.Distinct().ToList();

            var isValidTeamUser = await _workflowRepository.IsValidTeamUser(uniqueCardIds, assignRangeDto.UserId);
            if (!isValidTeamUser)
            {
                throw new AppException(ErrorCode.NotFound, "User not found",
                    CardLabel.UserCannotBeAssigned);
            }

            var cards = await _cardRepository.FindByCardIdsAsync(uniqueCardIds);
            if (cards == null || cards.Count == 0)
                throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);

            if (cards.Count != uniqueCardIds.Count)
                throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);

            return await ApplyAssignRangeAsync(assignRangeDto.UserId, cards);
        }

        /// <summary>
        /// Updates the step and status of a card.
        /// </summary>
        /// <param name="updateCardStepStatusDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UpdateStepAndStatus(
            UpdateCardStepStatusDto updateCardStepStatusDto,
            string tenant,
            string email
        )
        {
            var cards = await _cardRepository.FindCardOrBatchWithDocumentAsync(updateCardStepStatusDto.CardId);
            if (cards == null || cards.Count == 0)
                throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);

            var leadCard = cards[0];
            var previousStepId = leadCard.StepId;
            var previousStatusId = leadCard.StatusId;

            var step = await _stepRepository.FindByOrderAndWorkflowId(
                updateCardStepStatusDto.NextStepOrder,
                updateCardStepStatusDto.WorkflowId
            ) ?? throw new AppException(ErrorCode.NotFound, "Step not found", StepLabel.NotFound);

            Card.UpdateStepAndStatus(cards, step.Id, card => card.IsRejected() ? previousStatusId : step.StatusId);

            var cardWorkflows = cards.Select(card => (card.Id, updateCardStepStatusDto.WorkflowId, card.DocumentId)).ToList();
            if (cardWorkflows.Count > 0)
            {
                await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Advancement);
            }

            var result = await _cardRepository.UpdateList(cards);
            if (result)
            {
                foreach (var tempCard in cards)
                {
                    try
                    {
                        var automationServicesDto = new AutomationServicesDto
                        (
                            0,
                            tempCard.Id,
                            tenant,
                            email,
                            tempCard.Document!.ReferenceFile,
                            step.Id
                        );

                        await _automationServices.StartExecutionByCardAsync(automationServicesDto);
                    }
                    catch
                    {
                        tempCard.UpdateStepAndStatus(previousStepId, previousStatusId);
                        _cardRepository.Update(tempCard);
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Updates only the status of a card, keeping the same step. Does not trigger automation.
        /// </summary>
        /// <param name="updateCardStatusDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<bool> UpdateStatus(UpdateCardStatusDto updateCardStatusDto)
        {
            var cards = await _cardRepository.FindCardOrBatchWithStepWorkflowAsync(updateCardStatusDto.CardId);
            if (cards == null || cards.Count == 0)
                throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);

            foreach (var card in cards)
                card.UpdateStepAndStatus(card.StepId, updateCardStatusDto.StatusId);

            var cardWorkflows = cards.Where(card => card.Step != null).Select(card => (card.Id, card.Step!.WorkflowId, card.DocumentId)).ToList();
            if (cardWorkflows.Count > 0)
            {
                await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Finalize);
            }

            return await _cardRepository.UpdateList(cards);
        }

        /// <summary>
        /// Returns document information grouped by processing steps with extracted data.
        /// </summary>
        /// <param name="cardId">Card ID to analyze</param>
        /// <param name="headersDto">Request headers</param>
        /// <returns>Document with steps and extracted fields</returns>
        /// <exception cref="ArgumentException">Thrown when card is not found</exception>
        public async Task<DocumentAnalyzeStepsDto> FindByIdAnalyzeWithSteps(int cardId,
            HeadersDto headersDto)
        {
            var card = await _cardRepository.FindByIdWithDocumentAndWorkflow(cardId) ?? throw new AppException(ErrorCode.NotFound, $"Card {cardId} not found", null);
            var verifyAnswer = await VerifyCanAnswer(card);
            var document = card.Document ?? throw new ArgumentException("Document not found for the card");
            if (card.Step == null)
            {
                throw new ArgumentException($"Step not found for card {cardId}");
            }

            var workflow = await _workflowRepository.FindByIdForAnalyze(card.Step.WorkflowId) ??
                           throw new ArgumentException($"Workflow not found for card {cardId}. StepId: {card.StepId}, Step is null: false");

            var promptLabelCache = new Dictionary<int, string>();
            var questionnaireLabelCache = new Dictionary<int, string>();
            var steps = BuildStepsFromWorkflow(workflow, card, promptLabelCache, questionnaireLabelCache);
            var lastProcessedStepId = card.StepId.ToString();

            return new DocumentAnalyzeStepsDto
            {
                DocumentId = $"doc-{document.Id}",
                Name = document.Name,
                Description = document.Description,
                ReferenceFile = document.ReferenceFile,
                LastProcessedStepId = lastProcessedStepId,
                Steps = steps,
                CanAnswer = verifyAnswer
            };
        }

        /// <summary>
        /// Verify if can answer after embeddings and OCR
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        private async Task<bool> VerifyCanAnswer(CardAnalysisDto card)
        {
            var executions = await _stepToolExecutionRepository.FindByStepToolByCardIdAsync(card.Id);

            bool hasOcrReady = executions.Any(execution =>
                execution.StepTool.Tool.ToolType.Name.Equals(HandlersTypes.Ocr) &&
                execution.Status == StatusExecution.Ready);

            bool hasEmbeddingsReady = executions.Any(execution =>
                execution.StepTool.Tool.ToolType.Name.Equals(HandlersTypes.Embeddings) &&
                execution.Status == StatusExecution.Ready);

            if (hasOcrReady && hasEmbeddingsReady)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Prepare steps from workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private List<DocumentStepDto> BuildStepsFromWorkflow(
            Workflow workflow,
            CardAnalysisDto card,
            Dictionary<int, string> promptLabelCache,
            Dictionary<int, string> questionnaireLabelCache)
        {
            var steps = new List<DocumentStepDto>();
            var workflowSteps = workflow.Steps.OrderBy(s => s.Order).ToList();

            foreach (var step in workflowSteps)
            {
                var stepDto = CreateStepDto(step);
                PopulateStepOutputs(stepDto, step, card, promptLabelCache, questionnaireLabelCache);
                steps.Add(stepDto);
            }

            return steps;
        }

        /// <summary>
        /// Create a new DocumentStepDto
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        private DocumentStepDto CreateStepDto(Step step)
        {
            return new DocumentStepDto
            {
                Id = step.Id.ToString(),
                Name = step.Name,
                Outputs = new List<ExtractedFieldDto>()
            };
        }

        /// <summary>
        /// Populate StepOutputs by stepDto, step and card
        /// </summary>
        /// <param name="stepDto"></param>
        /// <param name="step"></param>
        /// <param name="card"></param>
        private void PopulateStepOutputs(
            DocumentStepDto stepDto,
            Step step,
            CardAnalysisDto card,
            Dictionary<int, string> promptLabelCache,
            Dictionary<int, string> questionnaireLabelCache)
        {
            foreach (var stepTool in step.StepTools.OrderBy(st => st.Order))
            {
                var outputs = card.Outputs?
                    .Where(o => o.StepToolId == stepTool.Id)
                    .ToList();

                if (outputs is null || outputs.Count <= 0)
                {
                    continue;
                }

                foreach (var output in outputs)
                {
                    if (ShouldSkipOutput(output))
                        continue;

                    var extractedFields = ParseOutput(output, promptLabelCache, questionnaireLabelCache);
                    stepDto.Outputs.AddRange(extractedFields);
                }
            }
        }

        /// <summary>
        /// Skips output if is OCR or Embeddings
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private static bool ShouldSkipOutput(StepToolOutputAnalysesDto output)
        {
            if (output.StepTool?.Tool == null)
                return true;

            var toolTypeName = output.StepTool.Tool.ToolType;
            return toolTypeName == HandlersTypes.Ocr || toolTypeName == HandlersTypes.Embeddings;
        }

        /// <summary>
        /// Parse output to Json or ExtractedFieldDto
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        private List<ExtractedFieldDto> ParseOutput(
            StepToolOutputAnalysesDto output,
            Dictionary<int, string> promptLabelCache,
            Dictionary<int, string> questionnaireLabelCache)
        {
            var fields = new List<ExtractedFieldDto>();

            if (string.IsNullOrWhiteSpace(output.Value))
                return fields;

            var toolName = ResolveToolName(output, promptLabelCache, questionnaireLabelCache);

            if (TryParseJsonOutput(output.Value, out var jsonFields, output.Id, output.StepTool?.Tool?.ToolType,
                    toolName))
            {
                fields.AddRange(jsonFields);
            }
            else
            {
                fields.Add(new ExtractedFieldDto
                {
                    Label = output.StepTool?.Tool?.Name ?? "Unknown",
                    Value = output.Value,
                    IsEdited = false,
                    OutputId = output.Id,
                    OutputType = output.StepTool?.Tool?.ToolType ?? "None",
                    ToolName = toolName,
                });
            }

            return fields;
        }

        /// <summary>
        /// Resolves display name for output tool, using Prompt.Text/Name for prompt outputs.
        /// Falls back to tool name when prompt cannot be resolved.
        /// </summary>
        private string ResolveToolName(
            StepToolOutputAnalysesDto output,
            Dictionary<int, string> promptLabelCache,
            Dictionary<int, string> questionnaireLabelCache)
        {
            var fallbackToolName = output.StepTool?.Tool?.Name ?? string.Empty;
            var toolType = output.StepTool?.Tool?.ToolType;

            if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                return ResolvePromptToolName(output, promptLabelCache, fallbackToolName);
            }

            if (string.Equals(toolType, HandlersTypes.Quiz, StringComparison.OrdinalIgnoreCase))
            {
                return ResolveQuestionnaireToolName(output, questionnaireLabelCache, fallbackToolName);
            }

            return fallbackToolName;
        }

        /// <summary>
        /// Resolves the display name for prompt tool outputs using cached prompt text or name.
        /// </summary>
        /// <param name="output">The tool output that contains prompt parameter metadata.</param>
        /// <param name="promptLabelCache">Cache of prompt id to resolved prompt label.</param>
        /// <param name="fallbackToolName">Fallback tool name used when no prompt label is found.</param>
        /// <returns>The prompt text or name when available; otherwise the fallback tool name.</returns>
        private string ResolvePromptToolName(
            StepToolOutputAnalysesDto output,
            Dictionary<int, string> promptLabelCache,
            string fallbackToolName)
        {
            var promptId = ExtractPromptId(output);
            if (!promptId.HasValue)
            {
                return fallbackToolName;
            }

            if (!promptLabelCache.TryGetValue(promptId.Value, out var cachedPromptLabel))
            {
                var prompt = _promptServices.FindById(promptId.Value);
                if (!string.IsNullOrWhiteSpace(prompt?.Text))
                {
                    cachedPromptLabel = prompt.Text;
                }
                else if (!string.IsNullOrWhiteSpace(prompt?.Name))
                {
                    cachedPromptLabel = prompt.Name;
                }
                else
                {
                    cachedPromptLabel = string.Empty;
                }

                promptLabelCache[promptId.Value] = cachedPromptLabel;
            }

            return string.IsNullOrWhiteSpace(cachedPromptLabel) ? fallbackToolName : cachedPromptLabel;
        }

        /// <summary>
        /// Resolves the display name for questionnaire tool outputs using cached questionnaire titles.
        /// </summary>
        /// <param name="output">The tool output that contains questionnaire parameter metadata.</param>
        /// <param name="questionnaireLabelCache">Cache of questionnaire id to resolved title.</param>
        /// <param name="fallbackToolName">Fallback tool name used when no questionnaire title is found.</param>
        /// <returns>The questionnaire title when available; otherwise the fallback tool name.</returns>
        private string ResolveQuestionnaireToolName(
            StepToolOutputAnalysesDto output,
            Dictionary<int, string> questionnaireLabelCache,
            string fallbackToolName)
        {
            var questionnaireId = ExtractQuestionnaireId(output);
            if (!questionnaireId.HasValue)
            {
                return fallbackToolName;
            }

            if (!questionnaireLabelCache.TryGetValue(questionnaireId.Value, out var cachedQuestionnaireLabel))
            {
                var questionnaire = _questionnaireServices.FindById(questionnaireId.Value);
                cachedQuestionnaireLabel = questionnaire?.Title ?? string.Empty;
                questionnaireLabelCache[questionnaireId.Value] = cachedQuestionnaireLabel;
            }

            return string.IsNullOrWhiteSpace(cachedQuestionnaireLabel) ? fallbackToolName : cachedQuestionnaireLabel;
        }

        /// <summary>
        /// Extracts the prompt id from the first StepTool parameter value.
        /// Returns null when the value is missing or invalid.
        /// </summary>
        private static int? ExtractPromptId(StepToolOutputAnalysesDto output)
        {
            return ExtractFirstStepToolParameterId(output);
        }

        /// <summary>
        /// Extracts the questionnaire id from the first StepTool parameter value.
        /// Returns null when the value is missing or invalid.
        /// </summary>
        private static int? ExtractQuestionnaireId(StepToolOutputAnalysesDto output)
        {
            return ExtractFirstStepToolParameterId(output);
        }

        /// <summary>
        /// Extracts and parses the first StepTool parameter value as an integer identifier.
        /// </summary>
        /// <param name="output">The output containing StepTool parameters.</param>
        /// <returns>The parsed identifier when available and valid; otherwise <see langword="null"/>.</returns>
        private static int? ExtractFirstStepToolParameterId(StepToolOutputAnalysesDto output)
        {
            var rawId = output.StepTool?.Parameters?.FirstOrDefault()?.Value;
            if (string.IsNullOrWhiteSpace(rawId))
            {
                return null;
            }

            return int.TryParse(rawId, out var parsedId) ? parsedId : null;
        }

        /// <summary>
        /// Parse output to json
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fields"></param>
        /// <param name="id"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        private static bool TryParseJsonOutput(string value, out List<ExtractedFieldDto> fields,
            int id,
            string? outputType,
            string toolName)
        {
            fields = new List<ExtractedFieldDto>();

            if (!value.TrimStart().StartsWith("{") || !value.TrimEnd().EndsWith("}"))
                return false;

            try
            {
                var settings = new JsonSerializerSettings
                {
                    MaxDepth = 5,
                    DateParseHandling = DateParseHandling.None
                };

                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(value, settings);
                if (jsonObject != null && jsonObject.Count > 0)
                {
                    foreach (var kvp in jsonObject)
                    {
                        fields.Add(new ExtractedFieldDto
                        {
                            Label = kvp.Key,
                            Value = kvp.Value?.ToString() ?? string.Empty,
                            IsEdited = false,
                            OutputId = id,
                            OutputType = outputType ?? string.Empty,
                            ToolName = toolName,
                        });
                    }

                    return true;
                }
            }
            catch (JsonException ex)
            {
                throw new AppException(ErrorCode.DefaultError, ex.Message.ToString(), null);
            }

            return false;
        }

        /// <summary>
        /// Helper Method that returns a headerdto with card name and workflow name
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<CardHeaderDto> FindHeaderInfoAsync(int cardId)
        {
            var dto = await _cardRepository.FindHeaderInfoAsync(cardId);
            if (dto == null)
            {
                throw new AppException(Domain.Enum.ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);
            }

            return dto;
        }

        /// <summary>
        /// Gets all cards for a document including step and workflow data.
        /// </summary>
        /// <param name="documentId">The document ID to get cards for.</param>
        /// <returns>Read-only list of cards with step and workflow; empty list if none found.</returns>
        public async Task<IReadOnlyList<Card>> FindCardsByDocumentIdWithStepWorkflowAsync(int documentId)
        {
            var cards = await _cardRepository.FindByDocumentIdCardListWithStepWorkflowAsync(documentId);
            return cards ?? new List<Card>();
        }

        /// <summary>
        /// Asynchronously retrieves a collection of card batch data transfer objects associated with the specified
        /// document batch identifier.
        /// </summary>
        /// <remarks>If no card batches are associated with the provided document batch identifier, the
        /// method returns <see langword="null"/> instead of an empty collection.</remarks>
        /// <param name="documentBatchId">The unique identifier of the document batch for which to retrieve associated card batches. Must be a
        /// positive integer.</param>
        /// <returns>A collection of <see cref="CardBatchDto"/> objects representing the card batches linked to the specified
        /// document batch identifier, or <see langword="null"/> if no card batches are found.</returns>
        public async Task<ICollection<CardBatchDto>?> FindCardsByDocumentBatchId(int documentBatchId)
        {
            var cards = await _cardRepository.FindByDocumentBatchId(documentBatchId);
            if (cards is null || cards.Count <= 0)
            {
                return null;
            }

            return [.. cards.Select(card => new CardBatchDto
            {
                CardId = card.Id,
                DocumentId = card.DocumentId,
                DocumentName = card.Name
            })];
        }

        /// <summary>
        /// Initiates the reprocessing of a card by updating its status and triggering automation services for the
        /// specified tenant and user email.
        /// </summary>
        /// <remarks>This method updates the card's status before starting the automation process. The
        /// operation is performed asynchronously and may trigger additional workflows depending on the automation
        /// service configuration.</remarks>
        /// <param name="cardId">The unique identifier of the card to be reprocessed. Must correspond to an existing card.</param>
        /// <param name="tenant">The tenant context in which the card reprocessing should occur. Used to scope the operation.</param>
        /// <param name="email">The email address of the user associated with the card reprocessing. Used for notification or tracking
        /// purposes.</param>
        /// <returns>A task that represents the asynchronous reprocessing operation.</returns>
        /// <exception cref="AppException">Thrown if the specified card does not exist.</exception>
        public async Task<bool> ReprocessCard(int cardId, string tenant, string email)
        {
            var card = await _cardRepository.FindByIdWithDocumentAndStep(cardId) ?? throw new AppException(ErrorCode.NotFound, CardNotFoundMessage, CardLabel.NotFound);

            card.UpdateStatus(card.Step!.StatusId);
            _cardRepository.Update(card);

            var automationServicesDto = new AutomationServicesDto
            (
                0,
                card.Id,
                tenant,
                email,
                card.Document!.ReferenceFile,
                card.StepId
            );

            await _automationServices.ReprocessStepTool(automationServicesDto);
            return true;
        }

        /// <summary>
        /// Assigns <paramref name="userId"/> to every card in <paramref name="cards"/>, writes assign audit entries
        /// and persists with <see cref="ICardRepository.UpdateList"/>.
        /// </summary>
        private async Task<bool> ApplyAssignRangeAsync(Guid userId, List<Card> cards)
        {
            Card.UpdateAssignedUser(cards, userId);

            var cardWorkflows = cards.Select(card => (card.Id, card.Step!.WorkflowId, card.DocumentId)).ToList();
            if (cardWorkflows.Count > 0)
            {
                await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, AuditCardActionType.Assign);
            }

            return await _cardRepository.UpdateList(cards);
        }
    }
}
