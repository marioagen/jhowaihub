using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ICardRepository
    {
        Task<Card?> FindById(int id);
        bool Update(Card card);
        Task<bool> DeleteByDocumentIds(List<int> documentIds);
        Task<bool> ExistsStepsInUse(ICollection<int> ids);
        Task<ICollection<int>> FindActiveCardIdsInFirstStepAsync(IEnumerable<int> cardIds);
    }
}
