using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class DocumentServices : IDocumentServices
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentServices> _logger;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _config;
        private readonly IFunctionFileRetriever _functionFileRetriever;
        private const string ConfigKeyAccessName = "keyAccess";
        private const string FindingDocumentErrorMessage = "Error while finding document in database";

        public DocumentServices(IDocumentRepository documentRepository,
            ILogger<DocumentServices> logger,
            IMarketPlaceApi marketPlaceApi,
            IConfiguration config,
            IFunctionFileRetriever functionFileRetriever)
        {
            _documentRepository = documentRepository;
            _logger = logger;
            _marketPlaceApi = marketPlaceApi;
            _config = config;
            _functionFileRetriever = functionFileRetriever;
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
            try
            {
                return await _marketPlaceApi.CheckExceededPages(_config[ConfigKeyAccessName]!, emailCreator);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentServices)} in the {nameof(CheckerExceededPages)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
        }

        /// <summary>
        /// This method sends the current page  
        /// and search text to repository and return an DocumentPagedResultDto.
        /// </summary>
        /// <param name="documentPagedDataDto"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public DocumentPagedResultDto FindAllPaged(DocumentPagedDataDto documentPagedDataDto,
            string emailCreator)
        {
            try
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
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                    throw;
                _logger.LogError(ex, $"An exception occurred in the {nameof(DocumentServices)} in the {nameof(FindAllPaged)} method");
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
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
        /// Retrieves a document as a byte array based on the provided file GUID and tenant information.
        /// </summary>
        /// <param name="fileGuidId"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<FindDocumentDto> FindDocumentById(int id,
            string tenant)
        {
            try
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
            catch (Exception ex)
            {
                if (ex is ArgumentNullException or ArgumentException)
                    throw;
                _logger.LogError(ex, "An exception occurred in {Service}.{Method} method for documentId: {Id} and tenant: {Tenant}.",
                    nameof(DocumentServices), nameof(FindDocumentById), id, tenant);
                throw new AppException(ErrorCode.DefaultError, ex.Message, null);
            }
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

    }
}
