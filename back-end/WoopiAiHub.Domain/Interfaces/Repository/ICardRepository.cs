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
        bool DeleteByIds(List<int> cardIds);
        Task<bool> DeleteByDocumentIds(List<int> documentIds);
        Task<int> CountByStepsInUse(ICollection<int> ids);
        Task<ICollection<int>> FindActiveCardIdsInFirstStepAsync(IEnumerable<int> cardIds);
        Task<Card?> FindByDocumentIdCardAsync(int documentId);
        Task<List<Card>> FindByDocumentIdCardListAsync(int documentId);
        Task<List<Card>> FindByDocumentIdCardListWithStepWorkflowAsync(int documentId);
        Task<CardHeaderDto?> FindHeaderInfoAsync(int cardId);
        Task<ICollection<int>> FindCardIdsByDocumentIdsAsync(IEnumerable<int> documentIds);
        Task<List<Card>> FindByDocumentBatchId(int documentBatchId);
        Task<List<Card>?> FindCardOrBatchWithStepWorkflowAsync(int cardId);
        Task<List<Card>?> FindCardOrBatchWithDocumentAsync(int cardId);
    }
}
