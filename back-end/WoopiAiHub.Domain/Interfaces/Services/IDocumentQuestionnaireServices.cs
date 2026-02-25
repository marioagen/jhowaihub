using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentQuestionnaireServices
    {
        Task<string> InputDocument(DocumentInputDto documentInputDto,
                                   HeadersDto headersDto);

        Task<bool> InputQuestionnaire(DocumentQuestionnaireDto documentQuestionnaireDto,
                                      HeadersDto headersDto);
    }
}
