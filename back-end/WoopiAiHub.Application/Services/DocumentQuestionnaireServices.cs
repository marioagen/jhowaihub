using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Refit;
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
        private readonly IEmbeddingsApi _embbedingsApi;
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
            IEmbeddingsApi embbedingsApi,
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
            _embbedingsApi = embbedingsApi;
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
                var questionnaire = _questionnaireRepository.FindById(documentQuestionnaireDto.IdQuestionnaire)
                    ?? throw new AppException(ErrorCode.NotFound, "Questionnaire not found", null);

                foreach (var description in questionnaire.Questions.Select(u => u.Description))
                {
                    var customQueryRequestDto = await CreateCustomQueryRequestDto(description,
                            headersDto.Tenant,
                            headersDto.Language);
                    var apikey = _config["IndexerApiKey"]!;

                    var resultRequest = await _embbedingsApi.CustomQuery(headersDto.Tenant,
                        documentDb.ReferenceFile.ToString(),
                        customQueryRequestDto,
                        apikey);

                    await ProcessRequestCustomQuery(resultRequest,
                        documentQuestionnaireDto.IdDocument,
                        description,
                        headersDto.EmailCreator,
                        isFromQuestionnaire: true);
                }

                await CreateAuditLogForDocumentCardsAsync(documentQuestionnaireDto.IdDocument, AuditCardActionType.InputQuestionnaire);

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
                var customQueryRequestDto = await CreateCustomQueryRequestDto(documentInputDto.Input,
                    headersDto.Tenant,
                    headersDto.Language);

                var apikey = _config["IndexerApiKey"]!;

                var resultRequest = await _embbedingsApi.CustomQuery(headersDto.Tenant,
                    documentDb.ReferenceFile.ToString(),
                    customQueryRequestDto,
                    apikey);

                var textResponse = await ProcessRequestCustomQuery(resultRequest,
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
        /// Processes the HTTP response from the custom query API: deserializes the result, records usage, creates and saves document history, and returns the response text or throws on failure.
        /// </summary>
        private async Task<string> ProcessRequestCustomQuery(HttpResponseMessage resultRequest,
            int id,
            string input,
            string emailCreator,
            bool isFromQuestionnaire)
        {
            if (resultRequest.IsSuccessStatusCode)
            {
                var queryResponse = await resultRequest.Content.ReadAsStringAsync();
                var queryResponseModel = JsonConvert.DeserializeObject<QueryResponseModelRefitDto>(queryResponse);

                var userId = _userRepository.FindIdByEmail(emailCreator);
                var userIdOrNull = (userId == Guid.Empty) ? (Guid?)null : userId;
                var historyType = isFromQuestionnaire ? DocumentHistoryTypeInputQuestionnaire : DocumentHistoryTypeDocumentInput;
                var documentHistoryForDb = CreateDocumentHistoryForDb(id,
                    queryResponseModel!.response,
                    input,
                    historyType,
                    userIdOrNull);
                foreach (var usage in queryResponseModel.Usage)
                {
                    await _usageDailyServices.AddByValuesAsync(MetricNames.Token, emailCreator, usage.Total_usage ?? 0,
                        usage.Model);
                }

                _documentHistoryServices.Create(documentHistoryForDb);

                return queryResponseModel.response;
            }
            else if (resultRequest.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                throw new FileNotFoundException("The file was not found in the llmindexer weaviate");
            }
            else
            {
                throw new AppException(ErrorCode.RefitApiError, "Error while sending question to Embeddings API", null);
            }
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
        private async Task<CustomQueryRequestRefitDto> CreateCustomQueryRequestDto(string input,
            string tenantName,
            string language)
        {
            var tenant = await _tenantCacheServices.FindTenantAsync(tenantName);

            return new CustomQueryRequestRefitDto
            {
                Question = input,
                Model = tenant!.Model,
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
