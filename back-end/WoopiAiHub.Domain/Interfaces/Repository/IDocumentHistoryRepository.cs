using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentHistoryRepository
    {
        bool Create(DocumentHistory documentHistory);
        IEnumerable<DocumentHistory> FindById(int idDocument);
        bool Delete(int idDocument);
        public bool UpdateHistory(UpdateHistoryDto updateHistoryDto);
    }
}
