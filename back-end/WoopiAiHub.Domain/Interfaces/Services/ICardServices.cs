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
        Task<bool> AssignRange(Guid userId, int cardId);
        Task<bool> AssignRangeAsync(AssignRangeDto request);
        Task<DocumentAnalyzeStepsDto> FindByIdAnalyzeWithSteps(int cardId,
            HeadersDto headersDto);
        Task<CardHeaderDto> FindHeaderInfoAsync(int cardId);
        Task<IReadOnlyList<Card>> FindCardsByDocumentIdWithStepWorkflowAsync(int documentId);
        Task<ICollection<CardBatchDto>?> FindCardsByDocumentBatchId(int documentBatchId);
        Task SetFailingCard(int cardId, string? email);
        Task<bool> ReprocessCard(int cardId, string tenant, string email);
    }
}
