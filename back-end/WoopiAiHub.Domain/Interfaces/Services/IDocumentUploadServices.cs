using WoopiAiHub.Application.Dto;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IDocumentUploadServices
    {
        Task ProcessChunks(RequestCreateDocumentDto requestCreateDocumentDto, string tenant);
    }
}
