using WoopiAiHub.Domain.DTOs.Request;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAnonimizationServices
    {
        Task<bool> ProcessAnonimization(int documentId, HeadersDto headersDto);
    }
}
