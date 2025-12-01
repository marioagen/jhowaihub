namespace WoopiAiHub.Domain.DTOs.Response.Automation
{
    public record class UsageUnitDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int? UsageTypeId { get; init; }
        public string UsageTypeName { get; init; } = string.Empty;
        public int? ModelEmbeddingId { get; init; }
        public string ModelEmbeddingName { get; init; } = string.Empty;
        public decimal Value { get; init; }
    }
}
