using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Hubs;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Infrastructure.Multitenancy;

namespace WoopiAiHub.Application.Services
{
    public class AnonymizationServices(
        IDocumentServices documentServices,
        IAnonymizationApi anonymizationApi,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IAuditCardService auditCardService,
        IDocumentAnonymizationRepository documentAnonymizationRepository,
        ITenantContextService tenantContextService,
        IServiceScopeFactory scopeFactory,
        ILogger<AnonymizationServices> logger
    ) : IAnonymizationServices
    {
        private readonly IDocumentServices _documentServices = documentServices;
        private readonly IAnonymizationApi _anonymizationApi = anonymizationApi;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IAuditCardService _auditCardService = auditCardService;
        private readonly IDocumentAnonymizationRepository _documentAnonymizationRepository = documentAnonymizationRepository;
        private readonly ITenantContextService _tenantContextService = tenantContextService;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<AnonymizationServices> _logger = logger;

        /// <summary>
        /// Initiates the anonymization process for a specified document using the provided request and header
        /// information.
        /// </summary>
        /// <param name="requestDto">The request data containing the document identifier and anonymization parameters.</param>
        /// <param name="headersDto">The headers information, including tenant context, required to locate and process the document.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the document file is not found, the anonymization API token is not configured, the anonymization
        /// user ID is not configured, or the anonymization response does not contain a download URL.</exception>
        public async Task ProcessAnonymization(ProcessAnonymizationRequestDto requestDto, HeadersDto headersDto)
        {
            var document = await _documentServices.FindDocumentById(requestDto.DocumentId, headersDto.Tenant);

            if (document.BytesDocument is null || document.BytesDocument.Length == 0)
            {
                throw new InvalidOperationException("Document file not found.");
            }

            var token = _configuration["RefitExternalSettings:AnonymizationApiToken"];
            var userId = _configuration.GetValue<int?>("RefitExternalSettings:AnonymizationUserId");

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Anonymization API token is not configured.");
            }

            if (!userId.HasValue)
            {
                throw new InvalidOperationException("Anonymization User ID is not configured.");
            }

            var anonRequest = new AnonymizationRequestDto
            {
                Document = new AnonymizationDocumentRequestDto
                {
                    Name = document.DocumentName,
                    Upload = $"{document.DocumentName}.pdf"
                },
                UserId = userId.Value,
                UriResponse = _configuration["AnonymizationWebhook"] ?? throw new InvalidOperationException("Anonymization Webhook not provided"),
                AnonymizationType = (int?)requestDto.AnonymizationType,
                WoopiAiPromptId = requestDto.PromptId?.ToString(),
                WoopiAiDocumentId = requestDto.DocumentId,
                WoopiAiEmail = headersDto.EmailCreator,
                WoopiAiTenant = headersDto.Tenant
            };

            var authHeader = $"Basic {token}";
            var response = await _anonymizationApi.InitiateAnonymization(authHeader, anonRequest);

            if (string.IsNullOrEmpty(response.Document.Download))
            {
                throw new InvalidOperationException("Download URL not provided in anonymization response");
            }

            await UploadDocumentToUrl(response.Document.Download, document.BytesDocument);

            await _auditCardService.CreateAndSaveAsync(requestDto.CardId, requestDto.WorkflowId, requestDto.DocumentId, AuditCardActionType.AnonymizationRequest, headersDto.EmailCreator);
        }

        /// <summary>
        /// Upload document binary to the provided URL
        /// </summary>
        /// <param name="uploadUrl">URL to upload the document</param>
        /// <param name="documentBytes">Document bytes to upload</param>
        private async Task UploadDocumentToUrl(string uploadUrl, byte[] documentBytes)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            using var content = new ByteArrayContent(documentBytes);

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Headers.Add("x-ms-blob-type", "BlockBlob");

            var response = await httpClient.PutAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to upload document. Status: {StatusCode}, Error: {Error}",
                                 response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to upload document. Status: {response.StatusCode}");
            }
        }

        /// <summary>
        /// Processes the result of a document anonymization operation by updating the repository and notifying
        /// subscribers when the anonymization is ready.
        /// </summary>
        /// <param name="result">An object containing the details of the completed anonymization operation, including the document
        /// identifier, user email, and the URL of the anonymized document. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="AppException">Thrown if the document specified by the anonymization result cannot be found.</exception>
        public async Task ProcessAnonymizationResult(AnonymizationResultDto result)
        {
            using var scope = _scopeFactory.CreateScope();

            var connectionString = await _tenantContextService.FindConnectionStringAndHttpAcessorAsync(result.WoopiAiTenant, scope);
            var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpAccessor.HttpContext ??= new DefaultHttpContext();
            httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

            var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
            var documentAnonymizationRepository = scope.ServiceProvider.GetRequiredService<IDocumentAnonymizationRepository>();
            var hubNotifier = scope.ServiceProvider.GetRequiredService<IHubNotifier>();

            var document = documentRepository.FindById(result.WoopiAiDocumentId) ?? throw new AppException(ErrorCode.NotFound, "Document not found", null);

            var documentAnonymization = new DocumentAnonymization(
                0,
                DateTime.Now,
                document.Id,
                result.DocumentUrl
            );
            await documentAnonymizationRepository.CreateAsync(documentAnonymization);

            await hubNotifier.AnonymizationReadyAsync(result.WoopiAiEmail, result.WoopiAiDocumentId, result.DocumentUrl);
        }

        /// <summary>
        /// Retrieves a collection of anonymized document records associated with the specified document identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document for which to retrieve anonymized versions.</param>
        /// <returns>A collection of <see cref="DocumentAnonymizationDto"/> objects representing the anonymized documents linked
        /// to the specified document. The collection is empty if no anonymized documents are found.</returns>
        public async Task<ICollection<DocumentAnonymizationDto>> FindAnonymizedDocumentsByDocument(int documentId)
        {
            return await _documentAnonymizationRepository.FindAnonymizedDocumentsByDocument(documentId);
        }
    }
}
