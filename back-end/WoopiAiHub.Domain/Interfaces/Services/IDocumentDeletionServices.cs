using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentDeletionServices
    {
        Task<bool> Delete(List<int> ids, HeadersDto headersDto);
    }
}
