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

        /// <summary>
        /// Serialized as string to preserve trailing zeros (e.g. "0.000000790").
        /// JSON numbers in JavaScript lose trailing zeros after parsing.
        /// </summary>
        public string Value { get; init; } = "0";
    }
}
