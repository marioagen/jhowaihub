namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public class DocumentEmbeddingsResultDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string KeyMongoAccess { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalPages { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}
