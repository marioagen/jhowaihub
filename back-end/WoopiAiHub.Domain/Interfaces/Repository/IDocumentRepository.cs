using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentRepository
    {
        bool Create(Document document);
        bool Delete(List<int> ids);
        Document FindById(int id);

        bool ChangeStatus(int id,
            DocumentStatus documentStatus);

        public IQueryable<DocumentListItemDto> FindAllOrdered(DocumentPagedDataDto documentPagedDataDto, string email);
        int FindDocumentCount();
        IQueryable<string> FindHashById(List<int> ids);
        int FindDocumentIdByReferenceFile(string referenceFile);
        Document? FindByReferenceFile(string referenceFile);
        bool ClearWorkflowRelationships(List<int> documentIds);
        Task<List<int>> FindOrphanDocumentIdsByWorkflowAsync(int workflowId, List<int>? candidateDocumentIds = null);
    }
}
