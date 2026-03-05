using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ICardRepository
    {
        Task<Card?> FindById(int id);
        Task<Card?> FindByIdWithStatus(int id);
        Task<Card?> FindByIdWithDocument(int id);
        Task<CardAnalysisDto?> FindByIdWithDocumentAndWorkflow(int id);
        Task<Card?> FindByIdWithStepAndProfile(int id);
        Task<Card?> FindByIdWithStepWorkflow(int id);
        bool Update(Card card);
        bool UpdateList(List<Card> cards);
        Task<bool> DeleteByDocumentIds(List<int> documentIds);
        Task<bool> ExistsStepsInUse(ICollection<int> ids);
        Task<ICollection<int>> FindActiveCardIdsInFirstStepAsync(IEnumerable<int> cardIds);
        Task<Card?> FindByDocumentIdCardAsync(int documentId);
        Task<List<Card>> FindByDocumentIdCardListAsync(int documentId);
        Task<List<Card>> FindByDocumentIdCardListWithStepWorkflowAsync(int documentId);
        Task<CardHeaderDto?> FindHeaderInfoAsync(int cardId);
        Task<ICollection<int>> FindCardIdsByDocumentIdsAsync(IEnumerable<int> documentIds);
        Task<List<Card>> FindByDocumentBatchId(int documentBatchId);
        /// <summary>
        /// Returns the card (or all cards in its document batch) with Step and Workflow loaded.
        /// Use when operating on a single card or its batch in the same way (e.g. assign, unassign, update status).
        /// </summary>
        Task<List<Card>?> FindCardOrBatchWithStepWorkflowAsync(int cardId);
        /// <summary>
        /// Returns the card (or all cards in its document batch) with Document loaded.
        /// Use when step/status updates need document reference (e.g. advancement with automation).
        /// </summary>
        Task<List<Card>?> FindCardOrBatchWithDocumentAsync(int cardId);
    }
}
