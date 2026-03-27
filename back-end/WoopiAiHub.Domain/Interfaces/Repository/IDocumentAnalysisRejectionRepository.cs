using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentAnalysisRejectionRepository
    {
        Task<bool> CreateAsync(DocumentAnalysisRejection rejection);
        Task<bool> CreateRangeAsync(List<DocumentAnalysisRejection> rejections);
        Task<List<DocumentAnalysisRejectionDto>> FindByCardIdAsync(int cardId);
    }
}
