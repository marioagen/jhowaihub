using Microsoft.AspNetCore.Http;

namespace WoopiAiHub.Domain.DTOs.Refit
{
    public class RequestUploadDto
    {
        public IFormFile File { get; set; }
        public string ContentType { get; set; }
    }
}
