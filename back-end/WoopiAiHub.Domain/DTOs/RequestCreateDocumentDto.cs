using Microsoft.AspNetCore.Http;

namespace WoopiAiHub.Application.Dto
{
    public record RequestCreateDocumentDto(
        IFormFile Chunk,
        string Filename,
        bool IsLast,
        string Name,
        string Description,
        string EmailCreator,
        ICollection<int> Workflows,
        bool IsLastFile = false,
        bool IsDocumentBatch = false
    );
}

