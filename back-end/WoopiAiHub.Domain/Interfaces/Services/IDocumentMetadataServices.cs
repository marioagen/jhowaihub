using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentMetadataServices
    {
        Task<FindByIdAnalyzeDto> FindByIdAnalyze(int id, HeadersDto headersDto);

        Task<OcrTextResponseDto> FindOcrTextByDocumentId(int documentId);
    }
}
