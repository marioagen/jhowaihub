using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentBatchRepository
    {
        Task<DocumentBatch> CreateAsync(DocumentBatch batch);
    }
}
