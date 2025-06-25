using DocAnalyzer.Application.Dto;
using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.Enum;
using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Interfaces.Repository
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
