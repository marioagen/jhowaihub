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
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateTime? DateRenew { get; set; }
        public string Plan { get; set; } = string.Empty;
        public string AiGatewayKey { get; set; } = string.Empty;
        public string BillingId { get; set; } = string.Empty;
        public Guid? AiGatewayApplicationId { get; set; }
    }
}
