using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;
using Newtonsoft.Json;

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

        private const string CardNotFoundMessage = "Card not found";

        public CardServices(ICardRepository cardRepository,
                            IAuditCardService auditCardService,
                            IStepRepository stepRepository,
                            IAutomationServices automationServices,
                            IStepToolExecutionRepository stepToolExecutionRepository,
                            IWorkflowRepository workflowRepository)
        {
            _cardRepository = cardRepository;
            _auditCardService = auditCardService;
            _stepRepository = stepRepository;
            _automationServices = automationServices;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _workflowRepository = workflowRepository;
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

            return _cardRepository.UpdateList(cards);
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

            return _cardRepository.UpdateList(cards);
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

            var result = _cardRepository.UpdateList(cards);
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

            var result = _cardRepository.UpdateList(cards);
            return result;
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

            var steps = BuildStepsFromWorkflow(workflow, card);
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
        private List<DocumentStepDto> BuildStepsFromWorkflow(Workflow workflow, CardAnalysisDto card)
        {
            var steps = new List<DocumentStepDto>();
            var workflowSteps = workflow.Steps.OrderBy(s => s.Order).ToList();

            foreach (var step in workflowSteps)
            {
                var stepDto = CreateStepDto(step);
                PopulateStepOutputs(stepDto, step, card);
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
        private static void PopulateStepOutputs(DocumentStepDto stepDto, Step step, CardAnalysisDto card)
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

                    var extractedFields = ParseOutput(output);
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
        private static List<ExtractedFieldDto> ParseOutput(StepToolOutputAnalysesDto output)
        {
            var fields = new List<ExtractedFieldDto>();

            if (string.IsNullOrWhiteSpace(output.Value))
                return fields;

            if (TryParseJsonOutput(output.Value, out var jsonFields, output.Id, output.StepTool?.Tool?.ToolType))
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
                });
            }

            return fields;
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
            string? outputType)
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
    }
}
