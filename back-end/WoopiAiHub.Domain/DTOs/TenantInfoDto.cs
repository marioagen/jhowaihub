namespace WoopiAiHub.Domain.DTOs
{
    public class TenantInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public string RefineTemplate { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public int KValue { get; set; }
        public string Model { get; set; } = string.Empty;
        public string EmbeddingModelName { get; set; } = string.Empty;
        public int ChunkSize { get; set; }
        public string SearchMode { get; set; } = string.Empty;
        public string OcrModel { get; set; } = string.Empty;
    }
}
