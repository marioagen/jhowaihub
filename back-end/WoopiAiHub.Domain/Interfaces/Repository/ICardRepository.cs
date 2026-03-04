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
        Task<bool> DeleteByDocumentIds(List<int> documentIds);
        Task<bool> ExistsStepsInUse(ICollection<int> ids);
        Task<ICollection<int>> FindActiveCardIdsInFirstStepAsync(IEnumerable<int> cardIds);
        Task<Card?> FindByDocumentIdCardAsync(int documentId);
        Task<List<Card>> FindByDocumentIdCardListAsync(int documentId);
        /// <summary>
        /// Finds all cards for the document with Step and Workflow included.
        /// </summary>
        Task<List<Card>> FindByDocumentIdCardListWithStepWorkflowAsync(int documentId);
        Task<CardHeaderDto?> FindHeaderInfoAsync(int cardId);
        Task<ICollection<int>> FindCardIdsByDocumentIdsAsync(IEnumerable<int> documentIds);
    }
}
