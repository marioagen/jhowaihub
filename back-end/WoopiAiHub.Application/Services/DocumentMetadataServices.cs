using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class DocumentMetadataServices : IDocumentMetadataServices
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IStepToolOutputRepository _stepToolOutputRepository;
        private readonly ILogger<DocumentMetadataServices> _logger;

        private const string FindingDocumentErrorMessage = "Error while finding document in database";

        public DocumentMetadataServices(
            IDocumentRepository documentRepository,
            ICardRepository cardRepository,
            IStepToolOutputRepository stepToolOutputRepository,
            ILogger<DocumentMetadataServices> logger)
        {
            _documentRepository = documentRepository;
            _cardRepository = cardRepository;
            _stepToolOutputRepository = stepToolOutputRepository;
            _logger = logger;
        }

        /// <summary>
        /// This sends the id to the repository and returns document information.
        /// </summary>
        public FindByIdAnalyzeDto FindByIdAnalyze(int id, HeadersDto headersDto)
        {
            var result = _documentRepository.FindById(id);

            if (result == null)
            {
                var ex = new ArgumentException(FindingDocumentErrorMessage);
                _logger.LogError(ex,
                    $"An exception occurred in the {nameof(DocumentMetadataServices)} in the {nameof(FindByIdAnalyze)} method");
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
        /// Retrieves the concatenated OCR text for a document by checking if an OCR StepTool execution exists with status "Ready"
        /// </summary>
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

        private static StepToolExecution? FindReadyOcrExecution(Card card)
        {
            return card.Executions
                .FirstOrDefault(e => e.Status == StatusExecution.Ready &&
                                     e.StepTool != null &&
                                     e.StepTool.Tool != null &&
                                     e.StepTool.Tool.ToolType != null &&
                                     e.StepTool.Tool.ToolType.Name == HandlersTypes.Ocr);
        }

        private static string ExtractOcrTextFromOutput(string outputJson)
        {
            var embeddingsData = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(outputJson);

            if (embeddingsData?.DocumentEmbeddings == null || embeddingsData.DocumentEmbeddings.Count == 0)
                return string.Empty;

            return string.Join(Environment.NewLine + Environment.NewLine,
                embeddingsData.DocumentEmbeddings
                    .OrderBy(e => (e.Metadata as dynamic)?.PageNumber ?? 0)
                    .Select(e => e.Text));
        }
    }
}
