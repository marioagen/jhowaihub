using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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
using WoopiAiHub.Domain.Interfaces.Services.Automation;
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
        private readonly IValidator<RequestCreateDocumentDto> _documentDtoValidator;
        private readonly ILogger<DocumentServices> _logger;
        private readonly IEmbeddingsApi _embbedingsApi;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly IDocumentHistoryServices _documentHistoryServices;
        private readonly IStepToolExecutionRepository _stepToolExecutionRepository;
        private readonly IFileRepositoryApi _fileRepositoryApi;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private readonly IMemoryCache _cache;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly MessageQueues _messageQueues;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubNotifier _hubNotifier;
        private readonly IAutomationServices _automationServices;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly IStepToolRepository _stepToolRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUsageDailyServices _usageDailyServices;
        private const string ConfigKeyAccessName = "keyAccess";
        private const string KeyMongoAccessNotFoundMessage = "Could not find embbedings api key";
        private const string FindingDocumentErrorMessage = "Error while finding document in database";

        public DocumentServices(IDocumentRepository documentRepository,
                                IValidator<RequestCreateDocumentDto> documentDtoValidator,
                                ILogger<DocumentServices> logger,
                                IEmbeddingsApi embbedingsApi,
                                IMarketPlaceApi marketPlaceApi,
                                IConfiguration config,
                                IDocumentHistoryServices documentHistoryServices,
                                IFileRepositoryApi fileRepositoryApi,
                                IFunctionFileRetriever functionFileRetriever,
                                IStepToolExecutionRepository stepToolExecutionRepository,
                                IMemoryCache cache,
                                IQuestionnaireRepository questionnaireRepository,
                                ITenantCacheServices tenantCacheServices,
                                ITeamServices teamServices,
                                IOptions<MessageQueues> messageQueues,
                                IHubNotifier documentNotifier,
                                IUnitOfWork unitOfWork,
                                ICardRepository cardRepository,
                                IAutomationServices automationServices,
                                IWorkflowRepository workflowRepository,
                                IStepToolOutputRepository stepToolOutputRepository,
                                IStepToolRepository stepToolRepository,
                                IUsageDailyServices usageDailyServices)
        {
            _unitOfWork = unitOfWork;
            _cardRepository = cardRepository;
            _documentRepository = documentRepository;
            _documentDtoValidator = documentDtoValidator;
            _logger = logger;
            _embbedingsApi = embbedingsApi;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _documentHistoryServices = documentHistoryServices;
            _fileRepositoryApi = fileRepositoryApi;
            _functionFileRetriever = functionFileRetriever;
            _stepToolExecutionRepository = stepToolExecutionRepository;
            _cache = cache;
            _questionnaireRepository = questionnaireRepository;
            _tenantCacheServices = tenantCacheServices;
            _messageQueues = messageQueues.Value;
            _hubNotifier = documentNotifier;
            _automationServices = automationServices;
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
                _logger.LogError(ex, $"An argument exception occurred in the {nameof(DocumentServices)} in the {nameof(FindAllPaged)} method");
                throw ex;
            }
        }

        /// <summary>
        /// Processes the chunks of the document that will be saved.
        /// If it is the last part of the document, save it in the Database.
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <returns></returns>
        public async Task ProcessChunks(RequestCreateDocumentDto requestCreateDocumentDto,
                                        string tenant)
        {
            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var bytes = this.AddNewBytesToArrayChunks(requestCreateDocumentDto,
                                                      cacheOptions);

            if (requestCreateDocumentDto.IsLast)
            {
                var referenceFile = await this.FinalizeUploadAsync(requestCreateDocumentDto, bytes, tenant);
                _cache.Remove(requestCreateDocumentDto.Name);
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

            var hashList = _documentRepository.FindHashById(ids).ToList();
            _unitOfWork.BeginTransaction();
            try
            {
                var deleted = _documentRepository.Delete(ids);

                await Task.WhenAll(hashList.Select(hash =>
                    DeleteHash(hash, headersDto.Tenant)));

                await _cardRepository.DeleteByDocumentIds(ids);

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
        /// This method sends a question to questionnaire and gets a response
        /// It also requests the repository layer to save the question and answer history
        /// </summary>
        /// <param name="documentQuestionnaireDto"></param>
        /// <param name="headersDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> InputQuestionnaire(DocumentQuestionnaireDto documentQuestionnaireDto,
                                                   HeadersDto headersDto)
        {
            var documentDb = _documentRepository.FindById(documentQuestionnaireDto.IdDocument);
            var questionnaire = _questionnaireRepository.FindById(documentQuestionnaireDto.IdQuestionnaire);

            foreach (var description in questionnaire.Questions.Select(u => u.Description))
            {
                bool availableBalanceToQuestion = await ManagerConsumptionQuestions(headersDto.EmailCreator,
                                                                                    headersDto.Tenant,
                                                                                    false);
                if (availableBalanceToQuestion)
                {
                    var customQueryRequestDto = await this.CreateCustomQueryRequestDto(description,
                                                                                      headersDto.Tenant,
                                                                                      headersDto.Language);
                    var apikey = _config["IndexerApiKey"]!;

                    var resultRequest = await _embbedingsApi.CustomQuery(headersDto.Tenant,
                                                                         documentDb.ReferenceFile.ToString(),
                                                                         customQueryRequestDto,
                                                                         apikey);

                    await this.ProcessRequestCustomQuery(resultRequest,
                                                         documentQuestionnaireDto.IdDocument,
                                                         description,
                                                         headersDto.EmailCreator);
                }
                else
                {
                    throw new HttpException(402, "Payment required, missing credits to execute action ");
                }
            }
            return true;
        }

        /// <summary>
        /// This sends the id to the repository and returns document information.
        /// FinddocumentDto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public FindByIdAnalyzeDto FindByIdAnalyze(int id,
                                                  HeadersDto headersDto)
        {
            var result = _documentRepository.FindById(id);

            if (result == null)
            {
                var ex = new ArgumentException(FindingDocumentErrorMessage);
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentServices)} in the {nameof(FindByIdAnalyze)} method");
                throw ex;
            }

            var cards = _cardRepository.FindByDocumentIdCardListAsync(id).Result;
            var activeCard = cards.FirstOrDefault();

            return new FindByIdAnalyzeDto
            {
                Name = result.Name,
                Description = result.Description,
                ReferenceFile = result.ReferenceFile,
                CardId = activeCard?.Id
            };
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
        /// This method sends a query to the Embeddings API and gets a response
        /// It also requests the repository layer to save the question and answer history
        /// </summary>
        /// <param name="input"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> InputDocument(DocumentInputDto documentInputDto,
                                                HeadersDto headersDto)
        {
            bool availableBalanceToQuestion = await ManagerConsumptionQuestions(headersDto.EmailCreator,
                                                                                headersDto.Tenant,
                                                                                false);

            if (availableBalanceToQuestion)
            {
                var documentDb = _documentRepository.FindById(documentInputDto.Id);
                var customQueryRequestDto = await this.CreateCustomQueryRequestDto(documentInputDto.Input,
                                                                                   headersDto.Tenant,
                                                                                   headersDto.Language);

                var apikey = _config["IndexerApiKey"]!;

                var resultRequest = await _embbedingsApi.CustomQuery(headersDto.Tenant,
                                                                     documentDb.ReferenceFile.ToString(),
                                                                     customQueryRequestDto,
                                                                     apikey);

                var textResponse = await this.ProcessRequestCustomQuery(resultRequest,
                                                                        documentInputDto.Id,
                                                                        documentInputDto.Input,
                                                                        headersDto.EmailCreator);

                return textResponse;
            }

            throw new AppException(ErrorCode.NoCreditsAvailable, "No Credits to send a Question", null);
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

            await _hubNotifier.CardProgessAsync(email, execution.CardId, percent, execution.StepTool.StepId);
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
        /// It uploads the document PDF file to the fileRepository and after uploading 
        /// it saves the document data in the database.
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <returns></returns>
        /// 
        private async Task<string> FinalizeUploadAsync(RequestCreateDocumentDto requestCreateDocumentDto,
                                                       Byte[] chunks,
                                                       string tenant)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                await _documentDtoValidator.ValidateAndThrowAsync(requestCreateDocumentDto);
                var formFile = new FormFile(new MemoryStream(chunks),
                                            0,
                                            chunks.Length,
                                            requestCreateDocumentDto.Filename,
                                            requestCreateDocumentDto.Filename);

                var referenceFile = await this.UploadFileToRepositoryApi(formFile,
                                                                        tenant);
                var workflows = await _workflowRepository.FindByIdsAsync(requestCreateDocumentDto.Workflows);
                var documentForDataBase = CreateDocumentForDb(requestCreateDocumentDto, workflows, referenceFile);

                ICollection<Card> cards = CreateDocumentCard(requestCreateDocumentDto, workflows);

                documentForDataBase.Cards = cards;
                _documentRepository.Create(documentForDataBase);

                var hasExecutions = await _automationServices.PrepareExecutionAsync(workflows!);
                var automationServicesDto = new AutomationServicesDto
                (
                    0,
                    0,
                    tenant,
                    requestCreateDocumentDto.EmailCreator,
                    referenceFile,
                    0
                );

                if (hasExecutions)
                {
                    await _automationServices.StartExecutionByWorkflowsAsync(automationServicesDto, workflows!);
                }

                _unitOfWork.Commit();
                return referenceFile;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// Processes the result of the question request for llmindexer
        /// </summary>
        /// <param name="resultRequest"></param>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task<string> ProcessRequestCustomQuery(HttpResponseMessage resultRequest,
                                                             int id,
                                                             string input,
                                                             string emailCreator)
        {
            if (resultRequest.IsSuccessStatusCode)
            {
                var queryResponse = await resultRequest.Content.ReadAsStringAsync();
                var queryResponseModel = JsonConvert.DeserializeObject<QueryResponseModelRefitDto>(queryResponse);

                var documentHistoryForDb = CreateDocumentHistoryForDb(id,
                                                                      queryResponseModel!.response,
                                                                      input);
                foreach (var usage in queryResponseModel.Usage)
                {
                    await _usageDailyServices.AddByValuesAsync(MetricNames.Token, emailCreator, usage.Total_usage??0, usage.Model);
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
        /// Manager Questions to Embedder
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <param name="tenant"></param>
        /// <returns>
        /// True => User have credits to send Questions
        /// False => User don't have credits do send Questions
        /// </returns>
        private async Task<bool> ManagerConsumptionQuestions(string emailCreator,
                                                             string tenant,
                                                             bool isKeyOrigin)
        {
            return await _marketPlaceApi.ManageConsumptionQuestions(
                _config[ConfigKeyAccessName]!,
                new ConsumptionQuestionsDto()
                {
                    Email = emailCreator,
                    Tenant = tenant,
                    IsKeyOrigin = isKeyOrigin
                }
            );
        }

        /// <summary>
        /// Creates an object of type CustomQueryRequestDto
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// Creates an object of type DocumentHistory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private static DocumentHistory CreateDocumentHistoryForDb(int id,
                                                           string output,
                                                           string input)
        {
            return new DocumentHistory
            (
                id,
                input,
                output,
                0,
                DateTime.Now
            );
        }

        /// <summary>
        /// Creates an Document type object to save in the database
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="referenceFile"></param>
        /// <param name="List<Workflow>"></param>
        /// <returns></returns>
        private static Document CreateDocumentForDb(RequestCreateDocumentDto requestCreateDocumentDto, List<Workflow> workflow, string referenceFile)
        {
            return new Document
            (
                requestCreateDocumentDto.Name,
                requestCreateDocumentDto.Description,
                referenceFile,
                (int)Domain.Enum.DocumentStatus.NotAnalyzed,
                true,
                requestCreateDocumentDto.EmailCreator,
                0,
                workflow,
                DateTime.Now
            );
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
                throw new ArgumentNullException(functionApiKeyAuth, "Function API key is missing in the configuration.");
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
        public async Task<MetaDataAutomationDto> ProcessEmbeddingsResult(DocumentEmbeddingsResultDto documentEmbeddingsResultDto)
        {
            var resultRegisterConsumption = await RegisterConsumptionPages(documentEmbeddingsResultDto);
            if (!resultRegisterConsumption)
                throw new AppException(ErrorCode.DefaultError, "Failed to send page consumption", null);

            var documentId = _documentRepository.FindDocumentIdByReferenceFile(documentEmbeddingsResultDto.ReferenceFile);
            if (documentId == 0)
            {
                throw new ArgumentException(FindingDocumentErrorMessage);
            }

            var execution = await _stepToolExecutionRepository
                .FindByStepToolIdAndCardIdAsync(documentEmbeddingsResultDto.Data.StepToolId, documentEmbeddingsResultDto.Data.CardId);
            await UpdateExecutionAsync(execution!, documentEmbeddingsResultDto.Email);
            await SaveStepToolOutputAsync(execution!, documentEmbeddingsResultDto.ReferenceFile);
            await this.ChangeStatus(documentId, DocumentStatus.Embeddings, documentEmbeddingsResultDto.Email);

            return documentEmbeddingsResultDto.Data;
        }

        /// <summary>
        /// Request the Refit interface to upload a file to the FileRepositoryApi
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> UploadFileToRepositoryApi(IFormFile formFile,
                                                             string tenant)
        {
            FileUploadSummaryDto resultUpload;

            using (MemoryStream ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                resultUpload = await _fileRepositoryApi.Upload(new ByteArrayPart(ms.ToArray(),
                                                                                 formFile.FileName),
                                                                                 tenant);
            }

            if (resultUpload is not null)
                return resultUpload.GuidId;
            else
                throw new AppException(ErrorCode.UploadFailed, "GuidId file reference returned null on upload FileRepository", null);
        }

        /// <summary>
        /// Adds the file chunk to the cache.
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        private byte[] AddNewBytesToArrayChunks(RequestCreateDocumentDto requestCreateDocumentDto,
                                                MemoryCacheEntryOptions cacheOptions)
        {
            byte[] newBytes;
            if (_cache.TryGetValue(requestCreateDocumentDto.Name,
                                   out byte[]? existingBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    requestCreateDocumentDto.Chunk.CopyTo(memoryStream);
                    byte[] bytesChunk = memoryStream.ToArray();
                    newBytes = existingBytes!.Concat(bytesChunk).ToArray();

                    _cache.Set(requestCreateDocumentDto.Name,
                               newBytes,
                               cacheOptions);
                }
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    requestCreateDocumentDto.Chunk.CopyTo(memoryStream);
                    byte[] bytesChunk = memoryStream.ToArray();
                    newBytes = bytesChunk;

                    _cache.Set(requestCreateDocumentDto.Name,
                               newBytes,
                               cacheOptions);
                }
            }

            return newBytes;
        }

        /// <summary>
        /// Ordenates the list of documents and returns a paged result
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="DocumentPagedDataDto"></param>
        /// <returns></returns>
        private DocumentPagedResultDto DocumentPagination(IQueryable<Document> query,
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
        /// Create card by a collections of teams
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="teams"></param>
        /// <returns></returns>
        private static List<Card> CreateDocumentCard(RequestCreateDocumentDto requestCreateDocumentDto, ICollection<Workflow> workflow)
        {
            return workflow
                .Select(w => w.Steps.OrderBy(s => s.Order).FirstOrDefault())
                .Where(step => step != null)
                .Select(step => new Card
                    (
                        0,
                        DateTime.UtcNow,
                        step!.Id,
                        0,
                        requestCreateDocumentDto.Filename,
                        step.StatusId,
                        true,
                        null
                    ))
                .ToList();
        }

        /// <summary>
        /// Extract normalized context from AnalyzeResult 
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        private async Task<List<DocumentEmbeddingsAddDto>> ExtractDocumentEmbeddingsAddDto(ProcessOcrResultDto processOcrResultDto)
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
        private async Task<DocumentEmbeddingsAddDto> CreateAddDocumentsEmbeddingsDtoAsync(ProcessOcrResultDto processOcrResultDto,
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

        /// <summary>
        /// Retrieves the concatenated OCR text for a document by checking if an OCR StepTool execution exists with status "Ready"
        /// </summary>
        /// <param name="documentId">The document ID</param>
        /// <returns>OcrTextResponseDto containing the OCR text if available</returns>
        public async Task<OcrTextResponseDto> FindOcrTextByDocumentId(int documentId)
        {
            var response = new OcrTextResponseDto { HasOcr = false };

            var document = _documentRepository.FindById(documentId);
            if (document == null)
                return response;

            response.ReferenceFile = document.ReferenceFile;

            var card = await _cardRepository.FindByDocumentIdCardAsync(documentId);
            if (card == null)
                return response;

            var ocrExecution = FindReadyOcrExecution(card);
            if (ocrExecution == null)
                return response;

            var outputJson = await _stepToolOutputRepository.FindByStepToolId(ocrExecution.StepToolId, card.Id);
            if (string.IsNullOrEmpty(outputJson))
                return response;

            var ocrText = ExtractOcrTextFromOutput(outputJson);
            if (!string.IsNullOrEmpty(ocrText))
            {
                response.Content = ocrText;
                response.HasOcr = true;
            }

            return response;
        }

        /// <summary>
        /// Finds the OCR execution with Ready status for a card
        /// </summary>
        /// <param name="card">The card to search in</param>
        /// <returns>OCR execution or null if not found</returns>
        private StepToolExecution? FindReadyOcrExecution(Card card)
        {
            return card.Executions
                .FirstOrDefault(e => e.Status == StatusExecution.Ready &&
                                    e.StepTool != null &&
                                    e.StepTool.Tool != null &&
                                    e.StepTool.Tool.ToolType != null &&
                                    e.StepTool.Tool.ToolType.Name == HandlersTypes.Ocr);
        }

        /// <summary>
        /// Extracts and concatenates OCR text from serialized output
        /// </summary>
        /// <param name="outputJson">Serialized StepToolOutput JSON</param>
        /// <param name="documentId">Document ID for logging</param>
        /// <returns>Concatenated OCR text or empty string if extraction fails</returns>
        private string ExtractOcrTextFromOutput(string outputJson)
        {
            var embeddingsData = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(outputJson);

            if (embeddingsData?.DocumentEmbeddings == null || !embeddingsData.DocumentEmbeddings.Any())
                return string.Empty;

            return string.Join(Environment.NewLine + Environment.NewLine,
                embeddingsData.DocumentEmbeddings
                    .OrderBy(e => (e.Metadata as dynamic)?.PageNumber ?? 0)
                    .Select(e => e.Text));
        }
    }
}
