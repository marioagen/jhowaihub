using Microsoft.AspNetCore.Http;

namespace DocAnalyzer.Application.Dto
{
    public class RequestCreateDocumentDto
    {
        public IFormFile Chunk { get; set; }
        public string Filename { get; set; } = string.Empty;
        public bool IsLast { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EmailCreator { get; set; } = string.Empty;
    }
}

