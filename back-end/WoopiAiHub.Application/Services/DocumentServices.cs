using Azure.AI.FormRecognizer.DocumentAnalysis;
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
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;
using WoopiAiHub.Infrastructure.Messaging.Configuration;


namespace WoopiAiHub.Application.Services
{
    public class DocumentServices : IDocumentServices
    {

        private readonly IDocumentRepository _documentRepository;
        private readonly IValidator<RequestCreateDocumentDto> _documentDtoValidator;
        private readonly ILogger<DocumentServices> _logger;
        private readonly IEmbeddingsApi _embbedingsApi;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly IDocumentHistoryServices _documentHistoryServices;
        private readonly IDocumentNormalizedServices _documentNormalizedServices;
        private readonly IFileRepositoryApi _fileRepositoryApi;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private readonly IOcrGoogle _ocrGoogle;
        private readonly IOcrAzure _ocrAzure;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQuestionnaireRepository _questionnaireRepository;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly ITeamServices _teamServices;
        private readonly IKeyGeneratorApi _keyGeneratorApi;
        private readonly MessageQueues _messageQueues;
        private readonly IMessagePublisher<ProcessOcrDto> _publisher;


        public DocumentServices(IDocumentRepository documentRepository,
                               IValidator<RequestCreateDocumentDto> documentDtoValidator,
                               ILogger<DocumentServices> logger,
                               IEmbeddingsApi embbedingsApi,
                               IMarketPlaceApi marketPlaceApi,
                               IConfiguration config,
                               IDocumentHistoryServices documentHistoryServices,
                               IFileRepositoryApi fileRepositoryApi,
                               IFunctionFileRetriever functionFileRetriever,
                               IDocumentNormalizedServices documentNormalizedServices,
                               IOcrGoogle ocrGoogle,
                               IOcrAzure ocrAzure,
                               IMemoryCache cache,
                               IHttpContextAccessor httpContextAccessor,
                               IQuestionnaireRepository questionnaireRepository,
                               ITenantCacheServices tenantCacheServices,
                               ITeamServices teamServices,
                               IKeyGeneratorApi keyGeneratorApi,
                               IMessagePublisher<ProcessOcrDto> publisher,
                               IOptions<MessageQueues> messageQueues)
        {
            _documentRepository = documentRepository;
            _documentDtoValidator = documentDtoValidator;
            _logger = logger;
            _embbedingsApi = embbedingsApi;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _documentHistoryServices = documentHistoryServices;
            _fileRepositoryApi = fileRepositoryApi;
            _functionFileRetriever = functionFileRetriever;
            _documentNormalizedServices = documentNormalizedServices;
            _ocrGoogle = ocrGoogle;
            _ocrAzure = ocrAzure;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _questionnaireRepository = questionnaireRepository;
            _tenantCacheServices = tenantCacheServices;
            _teamServices = teamServices;
            _keyGeneratorApi = keyGeneratorApi;
            _messageQueues = messageQueues.Value;
            _publisher = publisher;
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
            return await _marketPlaceApi.CheckExceededPages(_config["keyAccess"], emailCreator);
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
        /// This method sends a id to the File Repository and gets a filepath response
        /// It also read the file, send to OCR and finally Embeddings.
        /// </summary>
        /// <param name="documentAnalysisResponseDto"></param>
        /// <returns></returns>
        public async Task<bool> DocumentAnalysis(DocumentAnalysisResponseDto documentAnalysisResponseDto)
        {

            var document = _documentRepository.FindById(documentAnalysisResponseDto.Id);
            var functionApiKeyAuth = _config["RefitExternalSettings:FunctionApiKey"];

            if (string.IsNullOrEmpty(documentAnalysisResponseDto.Embeddings_model_name))
            {
                var tenant = await _tenantCacheServices.FindTenantAsync(documentAnalysisResponseDto.Tenant,
                                                                        ColTypeModule.WoopiAiHub);
                if (tenant == null || string.IsNullOrEmpty(tenant.EmbeddingModelName))
                {
                    throw new ArgumentNullException("Could not find embbedings model");
                }
                documentAnalysisResponseDto.Embeddings_model_name = tenant.EmbeddingModelName;
            }
            else
            {
                await this.DeleteHash(document.ReferenceFile,
                                      documentAnalysisResponseDto.Tenant,
                                      documentAnalysisResponseDto.KeyMongoAcess);
            }

            if (string.IsNullOrEmpty(functionApiKeyAuth))
                throw new ArgumentNullException("Could not find function api key");

            var resultRequestFunction = await _functionFileRetriever.Get(document.ReferenceFile,
                                                                         functionApiKeyAuth,
                                                                         documentAnalysisResponseDto.Tenant);

            var bytesFile = await resultRequestFunction.Content.ReadAsByteArrayAsync();
            var useOcrGoogle = _config.GetSection("UseOcrGoogle").Get<bool>();
            var normalizedContext = "";

            if (useOcrGoogle)
            {
                var result = await _ocrGoogle.ProcessResult(bytesFile);
                normalizedContext = await this.SendDocumentsEmbbeddingsOcrByGoogle(result,
                                                                                   document.ReferenceFile,
                                                                                   documentAnalysisResponseDto);
                await ManageConsumptionPages(documentAnalysisResponseDto,
                                             result.Count,
                                             false);
            }
            else
            {
                Stream stream = new MemoryStream(bytesFile);
                var result = await _ocrAzure.ProcessResult(stream, documentAnalysisResponseDto.Tenant);
                normalizedContext = await this.SendDocumentsEmbeddingsOcrByAzure(result,
                                                                                 document.ReferenceFile,
                                                                                 documentAnalysisResponseDto);
                await ManageConsumptionPages(documentAnalysisResponseDto,
                                             result.Pages.Count,
                                             false);
            }
            var normalizedDocument = _documentNormalizedServices.FindById(documentAnalysisResponseDto.Id, documentAnalysisResponseDto.EmailCreator);
            if (normalizedDocument is not null)
            {
                var documentNormalizedForDb = this.CreateDocumentNormalizedForDb(documentAnalysisResponseDto.Id, normalizedContext, normalizedDocument.Id);
                _documentNormalizedServices.Update(documentNormalizedForDb);
            }
            else
            {
                var documentNormalizedForDb = this.CreateDocumentNormalizedForDb(documentAnalysisResponseDto.Id, normalizedContext, 0);
                _documentNormalizedServices.Create(documentNormalizedForDb);
            }

            _documentRepository.ChangeStatus(documentAnalysisResponseDto.Id, DocumentStatus.Analyzed);

            return true;
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
                await PublishOcrDto(tenant,
                                    referenceFile,
                                    requestCreateDocumentDto.EmailCreator);
            }
        }

        /// <summary>
        /// This sends the ids to repository, that change the status and deletes hash
        /// of the document. Using soft delete idea.
        /// </summary>
        /// <param name="deleteDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(List<int> ids, HeadersDto headersDto)
        {
            var hashList = this.FindHashById(ids);
            var result = _documentRepository.Delete(ids);

            foreach (var hash in hashList)
            {
                await this.DeleteHash(hash,
                                      headersDto.Tenant,
                                      headersDto.KeyMongoAccess);
            }
            return result;

        }

        /// <summary>
        /// Find hash by ids data
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public List<string> FindHashById(List<int> ids)
        {
            var hashList = _documentRepository.FindHashById(ids);

            if (hashList.AsEnumerable().All(s => string.IsNullOrEmpty(s)))
            {
                throw new ArgumentException("Some ids are incompatible");
            }
            else
            {
                return hashList.ToList();
            }
        }

        /// <summary>
        /// This method sends a question to questionnaire and gets a response
        /// It also requests the repository layer to save the question and answer history
        /// </summary>
        /// <param name="idDocument"></param>
        /// <param name="idQuestionnaire"></param>
        /// <param name="emailCreator"></param>
        /// <param name="keyMongoAccess"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> InputQuestionnaire(DocumentQuestionnaireDto documentQuestionnaireDto,
                                                   HeadersDto headersDto)
        {
            if (string.IsNullOrEmpty(headersDto.KeyMongoAccess))
                throw new ArgumentNullException("Could not find emmbeddings api key");

            HttpContext context = _httpContextAccessor.HttpContext!;
            var tenant = context.Request.Headers[HeaderNames.XTenant].ToString();

            var documentDb = _documentRepository.FindById(documentQuestionnaireDto.IdDocument);
            var questionnaire = _questionnaireRepository.FindById(documentQuestionnaireDto.IdQuestionnaire);

            foreach (var description in questionnaire.Questions.Select(u => u.Description))
            {
                bool availableBalanceToQuestion = await ManagerConsumptionQuestions(headersDto.EmailCreator,
                                                                                    tenant,
                                                                                    false);
                if (availableBalanceToQuestion)
                {
                    var customQueryRequestDto = await this.CreateCustomQueryRequestDto(description,
                                                                                      tenant,
                                                                                      headersDto.Language);

                    var resultRequest = await _embbedingsApi.CustomQuery(documentDb.ReferenceFile.ToString(),
                                                                         customQueryRequestDto,
                                                                         headersDto.KeyMongoAccess);

                    await this.ProcessRequestCustomQuery(resultRequest,
                                                         documentQuestionnaireDto.IdDocument,
                                                         description);
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
                var ex = new ArgumentException("Error while finding document in database");
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentServices)} in the {nameof(FindByIdAnalyze)} method");
                throw ex;
            }

            return new FindByIdAnalyzeDto
            {
                Name = result.Name,
                Description = result.Description,
                ReferenceFile = result.ReferenceFile,
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
        public bool ChangeStatus(int id,
                                 string emailCreator)
        {
            return _documentRepository.ChangeStatus(id, DocumentStatus.Analyzed);
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
            if (string.IsNullOrEmpty(headersDto.KeyMongoAccess))
                throw new ArgumentNullException("Could not find emmbeddings api key");

            bool availableBalanceToQuestion = await ManagerConsumptionQuestions(headersDto.EmailCreator,
                                                                                headersDto.Tenant,
                                                                                false);

            if (availableBalanceToQuestion)
            {
                var documentDb = _documentRepository.FindById(documentInputDto.Id);
                var customQueryRequestDto = await this.CreateCustomQueryRequestDto(documentInputDto.Input,
                                                                                   headersDto.Tenant,
                                                                                   headersDto.Language);

                var resultRequest = await _embbedingsApi.CustomQuery(documentDb.ReferenceFile.ToString(),
                                                                     customQueryRequestDto,
                                                                     headersDto.KeyMongoAccess);

                var textResponse = await this.ProcessRequestCustomQuery(resultRequest,
                                                                        documentInputDto.Id,
                                                                        documentInputDto.Input);

                return textResponse;
            }

            throw new ApplicationException("No Credits to send a Question");
        }

        /// <summary>
        /// Processes the OCR result and extracts document embeddings.
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<IEnumerable<DocumentEmbeddingsAddDto>> ProcessOcrResult(ProcessOcrResultDto processOcrResultDto)
        {
            var keyAccess = _config["keyAccess"];
            if (string.IsNullOrEmpty(keyAccess))
            {
                throw new InvalidOperationException("KeyAccess is not configured in the application settings.");
            }

            var documentoId = _documentRepository.FindDocumentIdByReferenceFile(processOcrResultDto.ReferenceFile);
            if (documentoId == 0)
            {
                throw new ArgumentException("Error while finding document in database");
            }

            var documentEmbeddingsAddDtoList = await ExtractDocumentEmbeddingsAddDto(processOcrResultDto);

            var normalizedContext = new StringBuilder();
            foreach (var page in documentEmbeddingsAddDtoList)
            {
                normalizedContext.AppendLine(page.Text);
            }

            var normalizedDocument = _documentNormalizedServices.FindById(documentoId, processOcrResultDto.Email);
            if (normalizedDocument is not null)
            {
                var documentNormalized = CreateDocumentNormalized(documentoId, normalizedContext.ToString(), normalizedDocument.Id);
                _documentNormalizedServices.Update(documentNormalized);
            }
            else
            {
                var documentNormalized = CreateDocumentNormalized(documentoId, normalizedContext.ToString(), 0);
                _documentNormalizedServices.Create(documentNormalized);
            }

            _documentRepository.ChangeStatus(documentoId, DocumentStatus.OCR);

            return documentEmbeddingsAddDtoList;
        }

        /// <summary>
        /// realize the publish in rabbitMq to ocr queue
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task PublishOcrDto(string tenant,
                                        string referenceFile,
                                        string email)
        {
            var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenant,
                                                                        ColTypeModule.WoopiAiHub);
            if (string.IsNullOrEmpty(tenantInfo!.OcrModel))
            {
                throw new ArgumentException("Ocr not found");
            }

            var processOcrDto = new ProcessOcrDto
            {
                Tenant = tenant,
                ReferenceFile = referenceFile,
                Model = tenantInfo.OcrModel,
                Email = email,
                ResponseQueue = _messageQueues.OcrQueueAiHubResponse
            };

            await _publisher.PublishAsync(_messageQueues.OcrQueue, processOcrDto);
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
            _documentDtoValidator.ValidateAndThrow(requestCreateDocumentDto);
            var formFile = new FormFile(new MemoryStream(chunks),
                                        0,
                                        chunks.Length,
                                        requestCreateDocumentDto.Filename,
                                        requestCreateDocumentDto.Filename);

            var referenceFile = await this.UploadFileToRepositoryApi(formFile,
                                                                     tenant);
            var documentForDataBase = this.CreateDocumentForDb(requestCreateDocumentDto,
                                                               referenceFile);

            var teams = _teamServices.FindByIdsAndUser(requestCreateDocumentDto.TeamsIds,
                                                       requestCreateDocumentDto.EmailCreator);

            ICollection<Card> cards = CreateDocumentCard(requestCreateDocumentDto, teams);

            documentForDataBase.Cards = cards;
            documentForDataBase.Teams = teams;
            _documentRepository.Create(documentForDataBase);

            return referenceFile;
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
                                                             string input)
        {
            if (resultRequest.IsSuccessStatusCode)
            {
                var queryResponse = await resultRequest.Content.ReadAsStringAsync();
                var queryResponseModel = JsonConvert.DeserializeObject<QueryResponseModelRefitDto>(queryResponse);

                var documentHistoryForDb = this.CreateDocumentHistoryForDb(id,
                                                                           queryResponseModel.response,
                                                                           input);

                _documentHistoryServices.Create(documentHistoryForDb);

                return queryResponseModel.response;
            }
            else if (resultRequest.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                throw new FileNotFoundException("The file was not found in the llmindexer weaviate");
            }
            else
            {
                throw new Exception("Error while sending question to Embeddings API");
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
                _config["keyAccess"],
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
            var tenant = await _tenantCacheServices.FindTenantAsync(tenantName,
                                                                    ColTypeModule.WoopiAiHub);

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
        /// Creates an object of type DocumentHistory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private DocumentHistory CreateDocumentHistoryForDb(int id,
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
        /// Creates an object of type DocumentNormalized
        /// </summary>
        /// <param name="id"></param>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private DocumentNormalized CreateDocumentNormalizedForDb(int idDocument,
                                                                 string content,
                                                                 int id)
        {
            return new DocumentNormalized
            (
                idDocument,
                content,
                id,
                DateTime.Now
            );
        }

        /// <summary>
        /// Creates an Document type object to save in the database
        /// </summary>
        /// <param name="requestCreateDocumentDto"></param>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        private Document CreateDocumentForDb(RequestCreateDocumentDto requestCreateDocumentDto,
                                             string referenceFile)
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
                DateTime.Now
            );
        }

        /// <summary>
        /// Creates an object of type AddDocumentsRequestDto
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<AddDocumentsRequestRefitDto> CreateAddDocumentsRequestDtoAsync(string text,
                                                                                          string tenantName,
                                                                                          DocumentPage page,
                                                                                          string embeddings_model_name)
        {
            var tenant = await _tenantCacheServices.FindTenantAsync(tenantName,
                                                                    ColTypeModule.WoopiAiHub);

            if (string.IsNullOrEmpty(embeddings_model_name))
            {
                embeddings_model_name = tenant.EmbeddingModelName;
            }

            return new AddDocumentsRequestRefitDto
            {
                text = text,
                metadata = new { PageNumber = page.PageNumber },
                Tenant = tenantName,
                embeddings_model_name = embeddings_model_name,
                Chunk_size = tenant.ChunkSize
            };
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
                                     string tenant,
                                     string keyMongo)
        {

            var resultRequest = await _embbedingsApi.DeleteHash(hash,
                                                                tenant,
                                                                keyMongo);

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
                throw new ArgumentNullException("Function API key is missing in the configuration.");
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
                throw new ArgumentException("Error while finding document in database");
            }

            return _documentRepository.ChangeStatus(id, status);
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
                throw new Exception("GuidId file reference returned null on upload FileRepository");
        }

        /// <summary>
        /// Sends the text performed by OCR by Google to the Embeddings API
        /// </summary>
        /// <param name="result"></param>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        private async Task<String> SendDocumentsEmbbeddingsOcrByGoogle(ICollection<string> result,
                                                                       string referenceFile,
                                                                       DocumentAnalysisResponseDto documentAnalysisResponseDto)
        {
            var normalizedContext = new StringBuilder();
            var tenant = await _tenantCacheServices.FindTenantAsync(documentAnalysisResponseDto.Tenant,
                                                                    ColTypeModule.WoopiAiHub);

            if (string.IsNullOrEmpty(documentAnalysisResponseDto.Embeddings_model_name))
            {
                documentAnalysisResponseDto.Embeddings_model_name = tenant.EmbeddingModelName;
            }

            foreach (var textPage in result)
            {
                var emptyJson = new { };
                var documentRequestRefitDto = new AddDocumentsRequestRefitDto
                {
                    text = textPage,
                    metadata = emptyJson,
                    Tenant = documentAnalysisResponseDto.Tenant,
                    embeddings_model_name = documentAnalysisResponseDto.Embeddings_model_name,
                    Chunk_size = tenant.ChunkSize
                };
                normalizedContext.AppendLine(textPage);

                var apiEmbbeddingsKeyAuth = documentAnalysisResponseDto.KeyMongoAcess;

                if (string.IsNullOrEmpty(apiEmbbeddingsKeyAuth))
                    throw new ArgumentNullException("Could not find emmbeddings api key");

                await _embbedingsApi.AddDocuments(referenceFile,
                                                  documentRequestRefitDto,
                                                  apiEmbbeddingsKeyAuth);
            }

            return normalizedContext.ToString();
        }

        /// <summary>
        /// Sends the text performed by OCR by Azure to the Embeddings API
        /// </summary>
        /// <param name="result"></param>
        /// <param name="referenceFile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<String> SendDocumentsEmbeddingsOcrByAzure(AnalyzeResult result,
                                                                     string referenceFile,
                                                                     DocumentAnalysisResponseDto documentAnalysisResponseDto)
        {
            var normalizedContext = new StringBuilder();
            var apiEmbbeddingsKeyAuth = documentAnalysisResponseDto.KeyMongoAcess;

            var tablesByPage = result.Tables
                .GroupBy(table => table.BoundingRegions.Count > 0 ? table.BoundingRegions[0].PageNumber : 0)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var page in result.Pages)
            {
                var pageText = new StringBuilder($"----------- Página {page.PageNumber} do PDF -----------\n\n");

                var paragraphTexts = page.Lines.Select(line => line.Content).ToList();

                var pageTables = tablesByPage.ContainsKey(page.PageNumber)
                    ? tablesByPage[page.PageNumber]
                    : new List<DocumentTable>();

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

                normalizedContext.Append(pageText.ToString());

                if (string.IsNullOrEmpty(apiEmbbeddingsKeyAuth))
                    throw new ArgumentNullException("Could not find emmbeddings api key");

                AddDocumentsRequestRefitDto addDocumentRequest = await CreateAddDocumentsRequestDtoAsync(pageText.ToString(),
                                                                                              documentAnalysisResponseDto.Tenant,
                                                                                              page,
                                                                                              documentAnalysisResponseDto.Embeddings_model_name);
                await _embbedingsApi.AddDocuments(referenceFile,
                                                  addDocumentRequest,
                                                  apiEmbbeddingsKeyAuth);
            }

            return normalizedContext.ToString();

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
                                   out byte[] existingBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    requestCreateDocumentDto.Chunk.CopyTo(memoryStream);
                    byte[] bytesChunk = memoryStream.ToArray();
                    newBytes = existingBytes.Concat(bytesChunk).ToArray();

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
        private static List<Card> CreateDocumentCard(RequestCreateDocumentDto requestCreateDocumentDto, ICollection<Team> teams)
        {

            return teams
                .Where(t => t.Workflow != null)
                .Select(t => t.Workflow!.Steps.OrderBy(o => o.Order).FirstOrDefault())
                .Where(step => step != null)
                .Select(step => new Card
                    (
                        0,
                        DateTime.UtcNow,
                        step!.Id,
                        0,
                        requestCreateDocumentDto.Filename,
                        step.StatusId
                    ))
                .ToList();
        }

        /// <summary>
        /// Send Pages cosumed in Azure Or Google OCR
        /// </summary>
        /// <param name="emailCreator"></param>
        /// <param name="tenant"></param>
        /// <param name="pages"></param>
        /// <returns></returns>
        private async Task<bool> ManageConsumptionPages(DocumentAnalysisResponseDto documentAnalysisResponseDto,
                                                        int pages,
                                                        bool isKeyOrigin)
        {
            return await _marketPlaceApi.ManageConsumptionPages(
                _config["keyAccess"],
                new ConsumptionPagesDto()
                {
                    Email = documentAnalysisResponseDto.EmailCreator,
                    Tenant = documentAnalysisResponseDto.Tenant,
                    Pages = pages,
                    IsKeyOrigin = isKeyOrigin
                });
        }

        /// <summary>
        /// Extract normalized context from AnalyzeResult 
        /// </summary>
        /// <param name="processOcrResultDto"></param>
        /// <returns></returns>
        private async Task<List<DocumentEmbeddingsAddDto>> ExtractDocumentEmbeddingsAddDto(ProcessOcrResultDto processOcrResultDto)
        {
            var keyAccess = _config["keyAccess"]!;
            var apiEmbbeddingsKeyAuth = await _keyGeneratorApi.GetKey(keyAccess, processOcrResultDto.Tenant);

            if (string.IsNullOrEmpty(apiEmbbeddingsKeyAuth))
                throw new ArgumentNullException("Could not find emmbeddings api key");

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
                                                                                          ColTypeModule.WoopiAiHub,
                                                                                          apiEmbbeddingsKeyAuth);
                listDocument.Add(documentEmbeddingsAddDto);
            }

            return listDocument;
        }

        /// <summary>
        /// Create a new DocumentNormalized for the database.
        /// </summary>
        /// <param name="idDocument"></param>
        /// <param name="content"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static DocumentNormalized CreateDocumentNormalized(int idDocument,
                                                                   string content,
                                                                   int id)
        {
            return new DocumentNormalized
            (
                idDocument,
                content,
                id,
                DateTime.Now
            );
        }

        /// <summary>
        /// Creates an object of type AddDocumentsRequestDto
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<DocumentEmbeddingsAddDto> CreateAddDocumentsEmbeddingsDtoAsync(ProcessOcrResultDto processOcrResultDto,
                                                                                          string text,
                                                                                          CustomDocumentPage page,
                                                                                          ColTypeModule module,
                                                                                          string keyMongoAccess)
        {
            var tenant = await _tenantCacheServices.FindTenantAsync(processOcrResultDto.Tenant,
                                                                    module);
            return new DocumentEmbeddingsAddDto
            {
                ReferenceFile = processOcrResultDto.ReferenceFile,
                KeyMongoAccess = keyMongoAccess,
                Text = text,
                Metadata = new { PageNumber = page.PageNumber },
                Tenant = processOcrResultDto.Tenant,
                EmbeddingModelName = tenant.EmbeddingModelName,
                ChunkSize = tenant.ChunkSize,
                Email = processOcrResultDto.Email
            };
        }
    }
}
