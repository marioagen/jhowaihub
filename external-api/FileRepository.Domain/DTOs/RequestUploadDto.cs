using Microsoft.AspNetCore.Http;

namespace FileRepository.Domain.DTOs
{
    public class RequestUploadDto
    {
        public IFormFile File { get; set; }
        public string ContentType { get; set; }
    }
}
