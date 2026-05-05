namespace WoopiAiHub.Domain.DTOs
{
    public record UsageDailyDto(
        int UsageTypeId,
        int UsageCount,
        Guid UserId,
        int? ModelEmbeddingId,
        bool Processed = false,
        int? WorkflowId = null
    );
}
