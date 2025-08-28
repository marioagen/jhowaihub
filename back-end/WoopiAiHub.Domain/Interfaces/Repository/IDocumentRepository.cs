using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentRepository
    {
        bool Create(Document document);
        bool Delete(List<int> ids);
        async Task<bool> DeleteAsync(List<int> ids);
        Document FindById(int id);
        bool ChangeStatus(int id,
                          DocumentStatus documentStatus);
        IQueryable<Document> FindAllOrdered(DocumentPagedDataDto documentPagedDataDto, string email);
        int FindDocumentCount();
        IQueryable<string> FindHashById(List<int> ids);
        Task<IQueryable<string>> FindHashByIdAsync(List<int> ids);
        int FindDocumentIdByReferenceFile(string referenceFile);
    }
}
