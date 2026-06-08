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
        Task<bool> AssignRangeAsync(AssignRangeDto assignRangeDto);

        /// <summary>
        /// Finalizes multiple cards in bulk by updating their status to the given status id.
        /// Only cards that belong to the last step of their workflow should be submitted.
        /// </summary>
        /// <param name="request">Status id and the list of card ids to finalize.</param>
        /// <returns><see langword="true"/> if all cards were finalized successfully.</returns>
        Task<bool> FinalizeRangeAsync(FinalizeRangeDto request);
        Task<DocumentAnalyzeStepsDto> FindByIdAnalyzeWithSteps(int cardId,
            HeadersDto headersDto);
        Task<CardHeaderDto> FindHeaderInfoAsync(int cardId);
        Task<IReadOnlyList<Card>> FindCardsByDocumentIdWithStepWorkflowAsync(int documentId);
        Task<ICollection<CardBatchDto>?> FindCardsByDocumentBatchId(int documentBatchId);
        Task<bool> ReprocessCard(int cardId, string tenant, string email);
    }
}
