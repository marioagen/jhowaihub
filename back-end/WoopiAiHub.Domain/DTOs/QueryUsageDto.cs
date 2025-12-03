namespace WoopiAiHub.Domain.DTOs
{
    public record class QueryUsageDto
    {
        public string Model { get; init; } = string.Empty;
        public string Usage_unity { get; init; } = string.Empty;
        public int? Prompt_usage { get; init; }
        public int? Completion_usage { get; init; }
        public int? Total_usage { get; init; }
    }
}
