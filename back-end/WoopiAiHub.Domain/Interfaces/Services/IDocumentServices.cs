using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.DTOs.Messaging;


namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentServices
    {
        DocumentPagedResultDto FindAllPaged(DocumentPagedDataDto documentPagedDataDto,
                                            string emailCreator);

        Task ProcessChunks(RequestCreateDocumentDto requestCreateDocumentDto, 
                           string tenant);

        Task<string> InputDocument(DocumentInputDto documentInputDto,
                                   HeadersDto headersDto);

        Task<bool> InputQuestionnaire(DocumentQuestionnaireDto documentQuestionnaireDto,
                                      HeadersDto headersDto);

        FindByIdAnalyzeDto FindByIdAnalyze(int id,
                                           HeadersDto headersDto);

        Task<bool> ChangeStatus(int id,
                                DocumentStatus status,
                                string emailCreator);

        int FindDocumentCount();

        Task<bool> CheckerExceededPages(string emailCreator);

        Task<bool> Delete(List<int> ids,
                          HeadersDto headersDto);

        Task<FindDocumentDto> FindDocumentById(int id,
                                               string tenant);

        Task<bool> ChangeStatusByReferenceFile(string referenceFile,
                                               string emailCreator,
                                               DocumentStatus status);

        Task<MetaDataAutomationDto> ProcessOcrResult(ProcessOcrResultDto dto);
        Task<MetaDataAutomationDto> ProcessEmbeddingsResult(DocumentEmbeddingsResultDto documentEmbeddingsResultDto);
        Task<OcrTextResponseDto> FindOcrTextByDocumentId(int documentId);
    };
}
