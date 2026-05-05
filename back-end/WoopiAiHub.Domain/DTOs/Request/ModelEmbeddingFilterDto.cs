namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ModelEmbeddingFilterDto
    {
        public int Id { get; init; }
        public string? Start { get; init; }
        public string? End { get; init; }
        public List<int>? WorkflowIds { get; init; }
    }
}
