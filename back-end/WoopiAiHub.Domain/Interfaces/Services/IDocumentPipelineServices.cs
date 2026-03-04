using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentPipelineServices
    {
        Task<MetaDataAutomationDto> ProcessOcrResult(ProcessOcrResultDto dto);
        Task<MetaDataAutomationDto> ProcessEmbeddingsResult(DocumentEmbeddingsResultDto documentEmbeddingsResultDto);
        Task<WoopiAiHub.Domain.Models.Document?> InputToolQuestionnaire(DocumentEmbeddingsQueryResponseDto documentQuestionnaireDto);
    }
}
