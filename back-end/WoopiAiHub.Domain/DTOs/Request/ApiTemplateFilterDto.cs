namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ApiTemplateFilterDto
    {
        public string? OrderBy { get; set; } = null;
        public string? Input { get; set; } = null;
        public string? Method { get; set; } = null;
        public bool? EnableAccessFromMcp { get; set; } = null;
    }
}