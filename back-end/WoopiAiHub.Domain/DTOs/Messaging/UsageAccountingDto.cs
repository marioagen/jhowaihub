using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class UsageAccountingDto
    {
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UsageTypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public string? ModelEmbeddingName { get; set; }
        public int? WorkflowId { get; set; }
        public UsageDailyOrigin Origin { get; set; } = UsageDailyOrigin.WoopiAi;
    }
}
