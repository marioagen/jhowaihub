using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IDocumentAnonymizationRepository
    {
        Task<bool> CreateAsync(DocumentAnonymization documentAnonymization);
        Task<ICollection<DocumentAnonymizationDto>> FindAnonymizedDocumentsByDocument(int documentId);
    }
}
