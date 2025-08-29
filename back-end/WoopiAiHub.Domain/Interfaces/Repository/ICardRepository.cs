using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ICardRepository
    {
        Task<Card?> FindById(int id);
        bool Update(Card card);
        Task<bool> DeleteByDocumentId(int documentId);
        Task<bool> ExistsStepsInUse(ICollection<int> ids);
    }
}
