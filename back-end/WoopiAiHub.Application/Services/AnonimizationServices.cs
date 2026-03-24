using Google.Apis.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class AnonimizationServices(
        IDocumentServices documentServices,
        IServicesWoopiAiApi servicesWoopiAi,
        IConfiguration configuration,
        ILogger<AnonimizationServices> logger
    ) : IAnonimizationServices
    {
        private readonly IDocumentServices _documentServices = documentServices;
        private readonly IServicesWoopiAiApi _servicesWoopiAi = servicesWoopiAi;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AnonimizationServices> _logger = logger;

        /// <summary>
        /// Processes the anonymization of a document identified by its ID using the provided header information.
        /// </summary>\
        /// <param name="documentId">The unique identifier of the document to be anonymized.</param>
        /// <param name="headersDto">The header information containing tenant and creator details required for processing the anonymization.</param>
        /// <returns>A task that represents the asynchronous anonymization operation.</returns>
        public async Task<bool> ProcessAnonimization(int documentId, HeadersDto headersDto)
        {
            // TODO: VALIDAR SE TODAS AS VARIÁVEIS DE AMBIENTE NECESSÁRIAS ESTÃO PREENCHIDAS

            var document = await _documentServices.FindDocumentById(documentId, headersDto.Tenant);

            var process = await _servicesWoopiAi.ProcessAnonymization(
                $"{document.DocumentName}.pdf",
                new ByteArrayPart(document.BytesDocument!, $"{document.DocumentName}.pdf", "application/pdf"),
                _configuration["AnonimizationWebWook"] ?? string.Empty,
                null,
                headersDto.EmailCreator,
                _configuration["RefitExternalSettings:ServicesWoopiAiApiKey"] ?? string.Empty
            );

            if (!process.IsSuccessStatusCode)
            {
                var errorContent = await process.Content.ReadAsStringAsync();
                string message = $"Error on processing anonimization: ({(int)process.StatusCode}) {errorContent}";
                _logger.LogError(message);
                throw new AppException(ErrorCode.DefaultError, message);
            }

            return true;
        }
    }
}
