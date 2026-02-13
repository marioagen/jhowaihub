namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ApiTemplateStepToolCreateDto
    {
        public int StepToolId { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? QueryTemplate { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
    }
}
