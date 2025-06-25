using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Interfaces.Repository
{
    public interface IDocumentHistoryRepository
    {
        bool Create(DocumentHistory documentHistory);
        IEnumerable<DocumentHistory> FindById(int idDocument);
        bool Delete(int idDocument);
        public bool UpdateHistory(UpdateHistoryDto updateHistoryDto);
    }
}
