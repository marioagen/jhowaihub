using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface IDocumentNormalizedServices
    {
        bool Create(DocumentNormalized documentNormalized);
        DocumentNormalized FindById(int id,
                                    string emailCreator);
        int FindDocumentNormalizedCount();

        bool Update(DocumentNormalized documentNormalized);
    }
}
