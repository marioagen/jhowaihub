namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class UsageTypeFilterDto
    {
        public string UsageType { get; init; } = string.Empty;
        public string? Start { get; init; }
        public string? End { get; init; }
        public List<int>? WorkflowIds { get; init; }
    }
}
