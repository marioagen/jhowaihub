namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ApiTemplateCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? QueryTemplate { get; set; }
        public string HeaderTemplate { get; set; } = string.Empty;
        public string? BodyTemplate { get; set; }
    }
}
