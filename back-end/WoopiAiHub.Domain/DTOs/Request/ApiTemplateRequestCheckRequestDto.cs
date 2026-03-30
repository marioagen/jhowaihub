namespace WoopiAiHub.Domain.DTOs.Request
{
    public class ApiTemplateRequestCheckRequestDto
    {
        public int? TemplateId { get; set; }
        public ApiTemplateCreateDto? Draft { get; set; }
        public Dictionary<string, string>? Variables { get; set; }
        public string? TemplateName { get; set; }
        public string? Tenant { get; set; }
        public string? Email { get; set; }
        public int? ExecutionId { get; set; }
    }
}
