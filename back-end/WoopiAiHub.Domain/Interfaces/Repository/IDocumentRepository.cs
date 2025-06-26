using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentRepository
    {
        bool Create(Document document);
        bool Delete(List<int> ids);
        Document FindById(int id);
        bool ChangeStatus(int id);
        IQueryable<Document> FindAllOrdered(DocumentPagedDataDto documentPagedDataDto, string email);
        int FindDocumentCount();
        IQueryable<string> FindHashById(List<int> ids);
    }
}
