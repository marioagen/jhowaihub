namespace WoopiAiHub.Domain.DTOs
{
    public record KeyValueDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
