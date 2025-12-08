using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace WoopiAiHub.Application.Dto
{
    public record RequestCreateDocumentDto(
        IFormFile Chunk,
        string Filename,
        bool IsLast,
        string Name,
        string Description,
        string EmailCreator,
        ICollection<int> Workflows
    );
}

