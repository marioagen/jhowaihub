namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class DocumentEmbeddingsAddDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string KeyMongoAccess { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public dynamic? Metadata { get; set; }
        public string? Tenant { get; set; }
        public string EmbeddingModelName { get; set; } = string.Empty;
        public int ChunkSize { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
