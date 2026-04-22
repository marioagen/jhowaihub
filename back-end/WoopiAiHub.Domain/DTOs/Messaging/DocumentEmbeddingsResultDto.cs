using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class DocumentEmbeddingsResultDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string KeyMongoAccess { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalPages { get; set; }
        public int TotalUsage { get; set; }
        public string EmbeddingModelName { get; set; } = string.Empty;
        public MetaDataAutomationDto Data { get; set; }
    }
}
