namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ApiTemplateDto
    {
        public int? Id { get; set; }
        public DateTime? Created { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? QueryTemplate { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? Description { get; set; }
    }
}
