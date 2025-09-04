using Microsoft.AspNetCore.Http;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class FindByIdAnalyzeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
    }
}
