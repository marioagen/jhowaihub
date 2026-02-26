using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Refit;
using System.Net;
using System.Text;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.Services
{
    public class DocumentServices : IDocumentServices
    {
        private readonly ICardRepository _cardRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentServices> _logger;
        private readonly IEmbeddingsApi _embbedingsApi;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IFileRepositoryApi _fileRepositoryApi;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly MessageQueues _messageQueues;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotifier _hubNotifier;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUsageDailyServices _usageDailyServices;
        private const string ConfigKeyAccessName = "keyAccess";
        private const string FindingDocumentErrorMessage = "Error while finding document in database";
        public DocumentServices(IDocumentRepository documentRepository,
            ILogger<DocumentServices> logger,
            IEmbeddingsApi embbedingsApi,
            IMarketPlaceApi marketPlaceApi,
            IConfiguration config,
            IFileRepositoryApi fileRepositoryApi,
            IFunctionFileRetriever functionFileRetriever,
            IStepToolExecutionRepository stepToolExecutionRepository,
            ITenantCacheServices tenantCacheServices,
            IOptions<MessageQueues> messageQueues,
            IHubNotifier documentNotifier,
            IUnitOfWork unitOfWork,
            ICardRepository cardRepository,
            IWorkflowRepository workflowRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            IStepToolRepository stepToolRepository,
            IUsageDailyServices usageDailyServices)
        {
            _unitOfWork = unitOfWork;
            _cardRepository = cardRepository;
            _documentRepository = documentRepository;
            _logger = logger;
            _embbedingsApi = embbedingsApi;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _fileRepositoryApi = fileRepositoryApi;
            _functionFileRetriever = functionFileRetriever;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _tenantCacheServices = tenantCacheServices;
            _messageQueues = messageQueues.Value;
            _hubNotifier = documentNotifier;
            _workflowRepository = workflowRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _stepToolRepository = stepToolRepository;
            _usageDailyServices = usageDailyServices;
        }

        /// <summary>
        /// Checker if user has exceeded pages
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <returns>
        /// True => User exceeded Pages
        /// False => User don't exceeded Pages
        /// </returns>
        public async Task<bool> CheckerExceededPages(string emailCreator)
        {
            return await _marketPlaceApi.CheckExceededPages(_config[ConfigKeyAccessName]!, emailCreator);
        }

        /// <summary>
        /// This method sends the current page  
        /// and search text to repository and return an DocumentPagedResultDto.
        /// </summary>
        /// <param name="documentPagedDataDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DocumentPagedResultDto FindAllPaged(DocumentPagedDataDto documentPagedDataDto,
            string emailCreator)
        {
            if (documentPagedDataDto.Page > 0)
            {
                var totalList = _documentRepository.FindAllOrdered(documentPagedDataDto, emailCreator);
                var result = this.DocumentPagination(totalList, documentPagedDataDto);
                return result;
            }
            else
            {
                var ex = new ArgumentException("Invalid Page");
                _logger.LogError(ex,
                    $"An argument exception occurred in the {nameof(DocumentServices)} in the {nameof(FindAllPaged)} method");
                throw ex;
            }
        }

        /// <summary>
        /// Delete documents by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        public async Task<bool> Delete(List<int> ids, HeadersDto headersDto)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var referenceFilesToRemove = _documentRepository.FindHashById(ids).ToList();
            var hashList = referenceFilesToRemove;

            _unitOfWork.BeginTransaction();
            try
            {
                _documentRepository.ClearWorkflowRelationships(ids);

                var cardIds = await _cardRepository.FindCardIdsByDocumentIdsAsync(ids);
                if (cardIds.Any())
                {
                    _stepToolExecutionRepository.DeleteByCardIds(cardIds);
                    _stepToolOutputRepository.DeleteByCardIds(cardIds);
                }

                await _cardRepository.DeleteByDocumentIds(ids);
                var deleted = _documentRepository.Delete(ids);
                await Task.WhenAll(hashList.Select(hash => DeleteHash(hash, headersDto.Tenant)));

                if (referenceFilesToRemove.Any())
                {
                    await DeleteBlobFilesAsync(referenceFilesToRemove, headersDto.Tenant);
                }

                _unitOfWork.Commit();
                return deleted;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Return the status and name of an document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object FindStatusAndName(int id,
            string emailCreator)
        {
            var document = _documentRepository.FindById(id);

            var result = new
            {
                status = (int)document.Status,
                name = document.Name,
            };

            return result;
        }

        /// <summary>
        /// Change the current status of an document
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> ChangeStatus(int id,
            DocumentStatus status,
            string emailCreator)
        {
            var result = _documentRepository.ChangeStatus(id, status);

            return result;
        }

        /// <summary>
        /// Find documents and count
        /// </summary>
        /// <returns></returns>
        public int FindDocumentCount()
        {
            return _documentRepository.FindDocumentCount();
        }

        /// <summary>
        /// Processes the OCR result and extracts document embeddings.
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<MetaDataAutomationDto> ProcessOcrResult(ProcessOcrResultDto dto)
        {
            if (dto.Data.Equals(default(MetaDataAutomationDto)))
                return new MetaDataAutomationDto();

            var documentEmbeddings = await ExtractDocumentEmbeddingsAddDto(dto);

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(dto.Data.StepToolId, dto.Data.CardId);

            if (execution is null)
                return dto.Data;

            await UpdateExecutionAsync(execution, dto.Email);
            var dependentStepTool = await _stepToolRepository.FindDependentAsync(dto.Data.StepToolId);
            string embeddingsJson = JsonConvert.SerializeObject(new DocumentEmbeddingsDataDto
            {
                ResponseQueue = _messageQueues.EmbeddingQueueAiHubResponse,
                ReferenceFile = dto.ReferenceFile,
                DocumentEmbeddings = documentEmbeddings,
                Data = new MetaDataAutomationDto { CardId = dto.Data.CardId, StepToolId = dependentStepTool?.Id ?? 0 },
            });

            await SaveStepToolOutputAsync(execution, embeddingsJson);
            await UpdateDocumentStatusAsync(dto.ReferenceFile, dto.Email);

            return dto.Data;
        }

        /// <summary>
        /// Processes the questionnaire result from the tool and saves the result, token usage and history in the database.
        /// </summary>
        /// <param name="documentQuestionnaireDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<Document?> InputToolQuestionnaire(DocumentEmbeddingsQueryResponseDto documentQuestionnaireDto)
        {
            var documentDb = _documentRepository.FindByReferenceFile(documentQuestionnaireDto.ReferenceFile);
            if (documentDb == null)
            {
                return null;
            }

            _unitOfWork.BeginTransaction();
            try
            {
                var dataDto = System.Text.Json.JsonSerializer.Deserialize<MetaDataAutomationDto>(documentQuestionnaireDto.Data.ToString());

                var execution = await _stepToolExecutionRepository
                    .FindByStepToolIdAndCardIdAsync(dataDto.StepToolId, dataDto.CardId);

                await UpdateExecutionAsync(execution!, documentQuestionnaireDto.Email);
                await SaveStepToolOutputAsync(
                    execution!, 
                    System.Text.Json.JsonSerializer.Serialize(
                        documentQuestionnaireDto
                            .QuestionsAnswers
                                .Select(x => new QuestionAnswerDto {
                                    Id = x.Id,
                                    Question = x.Question,
                                    Answer = x.Answer
                                })
                                .ToList()));

                var usages = documentQuestionnaireDto.QuestionsAnswers.SelectMany(x => x.Usage)
                    .ToList();

                await _usageDailyServices.AddByRangeValuesAsync(
                    MetricNames.Token,
                    documentQuestionnaireDto.Email,
                    usages
                );

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }

            return documentDb;
        }


        /// <summary>
        /// Updates StepToolExecution status and send notification 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task UpdateExecutionAsync(StepToolExecution execution, string email)
        {
            var count = await _stepToolExecutionRepository.ExecutionsByStepIdCountAsync(execution.StepTool!.StepId,
                execution.CardId);
            var percent = ((double)execution.StepTool.Order / count) * 100;

            execution.UpdateStatusExecution(StatusExecution.Ready);
            await _stepToolExecutionRepository.UpdateAsync(execution);

            var tool = await _workflowRepository.FindToolByStepToolId(execution.StepTool.Id);

            await _hubNotifier.CardProgessAsync(email, execution.CardId, percent, execution.StepTool.StepId,
                tool != null ? tool.Name : string.Empty);
        }

        /// <summary>
        /// Updates StepToolExecution output
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="outputStepTool"></param>
        /// <returns></returns>
        private async Task SaveStepToolOutputAsync(StepToolExecution execution, string outputStepTool)
        {
            var output = new StepToolOutput(
                0,
                DateTime.Now,
                execution.StepToolId,
                execution.CardId,
                outputStepTool
            );

            await _stepToolOutputRepository.CreateAsync(output);
        }

        /// <summary>
        /// Updates document status
        /// </summary>
        /// <param name="referenceFile"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task UpdateDocumentStatusAsync(string referenceFile, string email)
        {
            var documentId = _documentRepository.FindDocumentIdByReferenceFile(referenceFile);
            await ChangeStatus(documentId, DocumentStatus.OCR, email);
        }

        /// <summary>
        /// Delete hash from Embeddings API
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="tenant"></param>
        /// <param name="keyMongo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DeleteHash(string hash,
            string tenant)
        {
            var apikey = _config["IndexerApiKey"]!;
            var resultRequest = await _embbedingsApi.DeleteHash(tenant,
                hash,
                tenant,
                apikey);

            if (!resultRequest.IsSuccessStatusCode && resultRequest.StatusCode != HttpStatusCode.NotFound)
            {
                throw new ArgumentException("Error while sending delete hash in Embeddings API");
            }
        }

        /// <summary>
        /// Deletes blob files from Azure Storage
        /// </summary>
        /// <param name="referenceFiles"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private async Task DeleteBlobFilesAsync(List<string> referenceFiles, string tenant)
        {
            foreach (var referenceFile in referenceFiles)
            {
                if (!string.IsNullOrEmpty(referenceFile))
                {
                    string blobPath = $"{tenant}/{referenceFile}";
                    await _fileRepositoryApi.Delete(blobPath);
                }
            }
        }

        /// <summary>
        /// Retrieves a document as a byte array based on the provided file GUID and tenant information.
        /// </summary>
        /// <param name="fileGuidId"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<FindDocumentDto> FindDocumentById(int id,
            string tenant)
        {
            var documentDb = _documentRepository.FindById(id);
            var functionApiKeyAuth = _config["RefitExternalSettings:FunctionApiKey"];

            if (string.IsNullOrEmpty(functionApiKeyAuth))
            {
                _logger.LogError("Function API key is missing in the configuration.");
                throw new ArgumentNullException(functionApiKeyAuth,
                    "Function API key is missing in the configuration.");
            }

            HttpResponseMessage document = await _functionFileRetriever.Get(documentDb.ReferenceFile,
                functionApiKeyAuth,
                tenant);

            byte[] bytesFile = await document.Content.ReadAsByteArrayAsync();

            return new FindDocumentDto
            {
                BytesDocument = bytesFile,
                ReferenceFile = documentDb.ReferenceFile
            };
        }

        /// <summary>
        /// Change the current status of an document by reference file
        /// </summary>
        /// <param name="referenceFile"></param>
        /// <param name="emailCreator"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<bool> ChangeStatusByReferenceFile(string referenceFile,
            string emailCreator,
            DocumentStatus status)
        {
            var id = _documentRepository.FindDocumentIdByReferenceFile(referenceFile);
            if (id == 0)
            {
                throw new ArgumentException(FindingDocumentErrorMessage);
            }

            return await this.ChangeStatus(id, status, emailCreator);
        }

        /// <summary>
        /// Process the result of the embeddings request and updates the document status.
        /// </summary>
        /// <param name="documentEmbeddingsResultDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<MetaDataAutomationDto> ProcessEmbeddingsResult(
            DocumentEmbeddingsResultDto documentEmbeddingsResultDto)
        {
            var documentId =
                _documentRepository.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile);
            if (documentId == 0)
            {
                throw new ArgumentException(FindingDocumentErrorMessage);
            }

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(documentEmbeddingsResultDto.Data.StepToolId,
                    documentEmbeddingsResultDto.Data.CardId);
            await UpdateExecutionAsync(execution!, documentEmbeddingsResultDto.Email);
            await SaveStepToolOutputAsync(execution!, documentEmbeddingsResultDto.ReferenceFile);
            await this.ChangeStatus(documentId, DocumentStatus.Embeddings, documentEmbeddingsResultDto.Email);

            return documentEmbeddingsResultDto.Data;
        }

        /// <summary>
        /// Ordenates the list of documents and returns a paged result
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="DocumentPagedDataDto"></param>
        /// <returns></returns>
        private DocumentPagedResultDto DocumentPagination(IQueryable<DocumentListItemDto> query,
            DocumentPagedDataDto dto)
        {
            int pageCount, currentPage;
            var totalListCount = query.Count();
            if (dto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                dto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / dto.PageSize);
                currentPage = dto.Page <= pageCount ? dto.Page : 1;

                query = query.Skip((currentPage - 1) * dto.PageSize)
                    .Take(dto.PageSize);
            }

            return new DocumentPagedResultDto
            {
                Content = query,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount
            };
        }

        
        /// <summary>
        /// Extract normalized context from AnalyzeResult 
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        private async Task<List<DocumentEmbeddingsAddDto>> ExtractDocumentEmbeddingsAddDto(
            ProcessOcrResultDto processOcrResultDto)
        {
            var apikey = _config["IndexerApiKey"]!;
            List<DocumentEmbeddingsAddDto> listDocument = new List<DocumentEmbeddingsAddDto>();

            var tablesByPage = processOcrResultDto.AnalyzeResult.Tables
                .GroupBy(table => table.BoundingRegions.Count > 0 ? table.BoundingRegions[0].PageNumber : 0)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var page in processOcrResultDto.AnalyzeResult.Pages)
            {
                var pageText = new StringBuilder($"----------- Página {page.PageNumber} do PDF -----------\n\n");

                var paragraphTexts = page.Lines.Select(line => line.Content).ToList();

                var pageTables = tablesByPage.TryGetValue(page.PageNumber, out List<CustomDocumentTable>? value)
                    ? value
                    : [];

                var tableTexts = pageTables.Select(table =>
                {
                    var tableContent = new StringBuilder($"\n--- Tabela ---\n");
                    foreach (var row in table.Cells.GroupBy(c => c.RowIndex))
                    {
                        var line = string.Join(" | ", row.OrderBy(c => c.ColumnIndex).Select(c => c.Content));
                        tableContent.AppendLine(line);
                    }

                    return tableContent.ToString();
                }).ToList();

                var remainingParagraphs = paragraphTexts
                    .Where(paragraph => !tableTexts.Any(table => table.Contains(paragraph)))
                    .ToList();

                pageText.AppendLine(string.Join(Environment.NewLine, remainingParagraphs));
                pageText.AppendLine(string.Join(Environment.NewLine, tableTexts));

                var documentEmbeddingsAddDto = await CreateAddDocumentsEmbeddingsDtoAsync(processOcrResultDto,
                    pageText.ToString(),
                    page,
                    apikey);
                listDocument.Add(documentEmbeddingsAddDto);
            }

            return listDocument;
        }

        /// <summary>
        /// Sends page consumption to the marketplace
        /// </summary>
        /// <param name="documentEmbeddingsResultDto"></param>
        /// <returns></returns>
        private async Task<bool> RegisterConsumptionPages(DocumentEmbeddingsResultDto documentEmbeddingsResultDto)
        {
            var consumption = new ConsumptionPagesDto
            {
                Email = documentEmbeddingsResultDto.Email,
                Pages = documentEmbeddingsResultDto.TotalPages,
                Tenant = documentEmbeddingsResultDto.Tenant,
                IsKeyOrigin = false
            };

            var keyAccess = _config["KeyAccess"]!;
            var result = await _marketPlaceApi.ManageConsumptionPages(keyAccess,
                consumption);

            return result;
        }

        /// <summary>
        /// Creates an object of type AddDocumentsRequestDto
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<DocumentEmbeddingsAddDto> CreateAddDocumentsEmbeddingsDtoAsync(
            ProcessOcrResultDto processOcrResultDto,
            string text,
            CustomDocumentPage page,
            string keyMongoAccess)
        {
            var tenant = await _tenantCacheServices.FindTenantAsync(processOcrResultDto.Tenant);
            return new DocumentEmbeddingsAddDto
            {
                ReferenceFile = processOcrResultDto.ReferenceFile,
                KeyMongoAccess = string.Empty,
                Text = text,
                Metadata = new { PageNumber = page.PageNumber },
                Tenant = processOcrResultDto.Tenant,
                EmbeddingModelName = tenant!.EmbeddingModelName,
                ChunkSize = tenant.ChunkSize,
                Email = processOcrResultDto.Email
            };
        }

    }
}
