namespace WoopiAiHub.Domain.DTOs.Request;

public record DateFilterDto
{
    public string? Start { get; init; }
    public string? End { get; init; }
}