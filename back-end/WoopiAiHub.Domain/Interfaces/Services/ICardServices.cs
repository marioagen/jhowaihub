using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ICardServices
    {
        Task<bool> UpdateStepAndStatus(UpdateCardStepStatusDto updateCardStepStatusDto,
            string tenant,
            string email);

        Task<bool> UpdateStatus(UpdateCardStatusDto updateCardStatusDto);
        Task<bool> AssignUser(UpdateAssignedUserDto updateAssingnedUserDto);
        Task<bool> UnassignUser(int cardId);

        Task<DocumentAnalyzeStepsDto> FindByIdAnalyzeWithSteps(int cardId,
            HeadersDto headersDto);

        Task<CardHeaderDto> FindHeaderInfoAsync(int cardId);
        Task<IReadOnlyList<Card>> GetCardsByDocumentIdWithStepWorkflowAsync(int documentId);
    }
}
