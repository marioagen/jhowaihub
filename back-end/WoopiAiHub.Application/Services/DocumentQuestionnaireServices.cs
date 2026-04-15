using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Services
{
    public class DocumentQuestionnaireServices : IDocumentQuestionnaireServices
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly IRagInvocationRouter _ragInvocationRouter;
        private readonly IConfiguration _config;
        private readonly IDocumentHistoryServices _documentHistoryServices;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IUsageDailyServices _usageDailyServices;
        private readonly IUserRepository _userRepository;
        private readonly ICardServices _cardServices;
        private readonly IAuditCardService _auditCardService;
        private readonly ILogger<DocumentQuestionnaireServices> _logger;

        private const int DocumentHistoryTypeInputQuestionnaire = 1;
        private const int DocumentHistoryTypeDocumentInput = 2;

        public DocumentQuestionnaireServices(
            IDocumentRepository documentRepository,
            IQuestionnaireRepository questionnaireRepository,
            IRagInvocationRouter ragInvocationRouter,
            IConfiguration config,
            IDocumentHistoryServices documentHistoryServices,
            ITenantCacheServices tenantCacheServices,
            IUsageDailyServices usageDailyServices,
            IUserRepository userRepository,
            ICardServices cardServices,
            IAuditCardService auditCardService,
            ILogger<DocumentQuestionnaireServices> logger)
        {
            _documentRepository = documentRepository;
            _questionnaireRepository = questionnaireRepository;
            _ragInvocationRouter = ragInvocationRouter;
            _config = config;
            _documentHistoryServices = documentHistoryServices;
            _tenantCacheServices = tenantCacheServices;
            _usageDailyServices = usageDailyServices;
            _userRepository = userRepository;
            _cardServices = cardServices;
            _auditCardService = auditCardService;
            _logger = logger;
        }

        /// <summary>
        /// This method sends a question to questionnaire and gets a response
        /// It also requests the repository layer to save the question and answer history
        /// </summary>
        public async Task<bool> InputQuestionnaire(DocumentQuestionnaireDto documentQuestionnaireDto,
            HeadersDto headersDto)
        {
            try
            {
                var documentDb = _documentRepository.FindById(documentQuestionnaireDto.IdDocument);
                var questionnaire = _questionnaireRepository.FindById(documentQuestionnaireDto.IdQuestionnaire);
                var tenantInfo = await _tenantCacheServices.FindTenantAsync(headersDto.Tenant);

                foreach (var description in questionnaire.Questions.Select(u => u.Description))
                {
                    var customQueryRequestDto =
                        CreateCustomQueryRequestDto(description, headersDto.Tenant, headersDto.Language, tenantInfo!);
                    var apikey = _config["IndexerApiKey"]!;

                    var executionResult = await _ragInvocationRouter.ExecuteCustomQueryAsync(
                        tenantInfo!,
                        documentDb.ReferenceFile.ToString(),
                        apikey,
                        headersDto.EmailCreator,
                        customQueryRequestDto,
                        CancellationToken.None);

                    await ProcessCustomQueryExecutionResult(executionResult,
                        documentQuestionnaireDto.IdDocument,
                        description,
                        headersDto.EmailCreator,
                        isFromQuestionnaire: true);
                }

                await CreateAuditLogForDocumentCardsAsync(documentQuestionnaireDto.IdDocument,
                    AuditCardActionType.InputQuestionnaire);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentQuestionnaireServices)} in the {nameof(InputQuestionnaire)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// This method sends a query to the Embeddings API and gets a response
        /// It also requests the repository layer to save the question and answer history
        /// </summary>
        public async Task<string> InputDocument(DocumentInputDto documentInputDto,
            HeadersDto headersDto)
        {
            try
            {
                var documentDb = _documentRepository.FindById(documentInputDto.Id);
                var tenantInfo = await _tenantCacheServices.FindTenantAsync(headersDto.Tenant);
                var customQueryRequestDto =
                    CreateCustomQueryRequestDto(documentInputDto.Input, headersDto.Tenant, headersDto.Language,
                        tenantInfo!);

                var apikey = _config["IndexerApiKey"]!;

                var executionResult = await _ragInvocationRouter.ExecuteCustomQueryAsync(
                    tenantInfo!,
                    documentDb.ReferenceFile.ToString(),
                    apikey,
                    headersDto.EmailCreator,
                    customQueryRequestDto,
                    CancellationToken.None);

                var textResponse = await ProcessCustomQueryExecutionResult(executionResult,
                    documentInputDto.Id,
                    documentInputDto.Input,
                    headersDto.EmailCreator,
                    isFromQuestionnaire: false);

                await CreateAuditLogForDocumentCardsAsync(documentInputDto.Id, AuditCardActionType.InputDocument);

                return textResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentQuestionnaireServices)} in the {nameof(InputDocument)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Persists usage, document history, and returns the model response text from a completed custom query execution.
        /// </summary>
        private async Task<string> ProcessCustomQueryExecutionResult(CustomQueryExecutionResult result,
            int id,
            string input,
            string emailCreator,
            bool isFromQuestionnaire)
        {
            var userId = _userRepository.FindIdByEmail(emailCreator);
            var userIdOrNull = (userId == Guid.Empty) ? (Guid?)null : userId;
            var historyType = isFromQuestionnaire ? DocumentHistoryTypeInputQuestionnaire : DocumentHistoryTypeDocumentInput;
            var documentHistoryForDb = CreateDocumentHistoryForDb(id,
                result.ResponseText,
                input,
                historyType,
                userIdOrNull);
            foreach (var usage in result.Usage)
            {
                await _usageDailyServices.AddByValuesAsync(MetricNames.Token, emailCreator, usage.Total_usage ?? 0,
                    usage.Model);
            }

            _documentHistoryServices.Create(documentHistoryForDb);

            return result.ResponseText;
        }

        /// <summary>
        /// Creates audit log entries for all cards associated with the document (no-op when document has no cards).
        /// </summary>
        private async Task CreateAuditLogForDocumentCardsAsync(int documentId, AuditCardActionType actionType)
        {
            var cards = await _cardServices.FindCardsByDocumentIdWithStepWorkflowAsync(documentId) ?? Array.Empty<Card>();
            var cardWorkflows = cards.Where(c => c.Step != null).Select(c => (c.Id, c.Step!.WorkflowId, c.DocumentId)).ToList();
            if (cardWorkflows.Count > 0)
            {
                await _auditCardService.CreateBatchAndSaveAsync(cardWorkflows, actionType);
            }
        }

        /// <summary>
        /// Builds a custom query request DTO from the input text and tenant/language, using cached tenant settings (model, k-value, template, etc.).
        /// </summary>
        private static CustomQueryRequestRefitDto CreateCustomQueryRequestDto(string input,
            string tenantName,
            string language,
            TenantInfoDto tenant)
        {
            return new CustomQueryRequestRefitDto
            {
                Question = input,
                Model = tenant.Model,
                kValue = tenant.KValue,
                Temperature = 0,
                Template = tenant.Template.Replace("{language}", language.ConvertLanguageCodeToName()),
                Refine_template = tenant.RefineTemplate,
                Max_tokens = tenant.MaxTokens,
                SearchMode = tenant.SearchMode,
                Tenant = tenantName
            };
        }

        /// <summary>
        /// Creates a <see cref="DocumentHistory"/> entity for persistence from the document id, input/output text, history type, and optional user id.
        /// </summary>
        private static DocumentHistory CreateDocumentHistoryForDb(int id,
            string output,
            string input,
            int type,
            Guid? userId)
        {
            return new DocumentHistory
            (
                id,
                input,
                output,
                0,
                DateTime.Now,
                type,
                userId
            );
        }
    }
}
