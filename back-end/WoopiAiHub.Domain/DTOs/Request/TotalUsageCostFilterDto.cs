namespace WoopiAiHub.Domain.DTOs.Request;

public record TotalUsageCostFilterDto
{
    public string? Start { get; init; }
    public string? End { get; init; }
    public List<int>? WorkflowIds { get; init; }
}
