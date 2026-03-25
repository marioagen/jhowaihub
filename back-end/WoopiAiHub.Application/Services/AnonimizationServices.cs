using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class AnonymizationServices(
        IDocumentServices documentServices,
        IAnonymizationApi anonymizationApi,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<AnonymizationServices> logger
    ) : IAnonymizationServices
    {
        private readonly IDocumentServices _documentServices = documentServices;
        private readonly IAnonymizationApi _anonymizationApi = anonymizationApi;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
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
                UriResponse = _configuration["AnonymizationWebWook"] ?? string.Empty,
                AnonymizationType = (int?)requestDto.AnonymizationType,
                WoopiAiPromptId = requestDto.PromptId?.ToString()
            };

            var authHeader = $"Basic {token}";
            var response = await _anonymizationApi.InitiateAnonymization(authHeader, anonRequest);

            if (string.IsNullOrEmpty(response.Document.Download))
            {
                throw new InvalidOperationException("Download URL not provided in anonymization response");
            }

            await UploadDocumentToUrl(response.Document.Download, document.BytesDocument);
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
    }
}
