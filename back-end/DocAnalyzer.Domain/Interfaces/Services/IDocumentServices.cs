using DocAnalyzer.Application.Dto;
using DocAnalyzer.Domain.DTOs.Request;
using DocAnalyzer.Domain.DTOs.Response;


namespace DocAnalyzer.Domain.Interfaces.Services
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

        Task<bool> DocumentAnalysis(DocumentAnalysisResponseDto documentAnalysisResponseDto);

        bool ChangeStatus(int id,
                          string emailCreator);

        object FindStatusAndName(int id,
                                 string emailCreator);

        int FindDocumentCount();

        Task<bool> CheckerExceededPages(string emailCreator);

        Task<bool> Delete(List<int> ids,
                          HeadersDto headersDto);

        Task<FindDocumentDto> FindDocumentById(int id,
                                               string tenant);
    };
}
