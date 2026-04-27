namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ApiTemplateDto
    {
        public int? Id { get; set; }
        public DateTime? Created { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? QueryTemplate { get; set; } = string.Empty;
        public string? HeaderTemplate { get; set; } = string.Empty;
        public string? BodyTemplate { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool EnableAccessFromMcp { get; set; } = false;
    }
}
