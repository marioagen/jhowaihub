using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentNormalizedServices
    {
        bool Create(DocumentNormalized documentNormalized);
        DocumentNormalized? FindById(int id);
        int FindDocumentNormalizedCount();
        bool Update(DocumentNormalized documentNormalized);
        void InsertOrUpdate(int documentId, string normalizedContext);
    }
}
