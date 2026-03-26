namespace WoopiAiHub.Domain.DTOs.Response
{
    public class ApiTemplateRequestTestsResponseDto
    {
        public int StatusCode { get; set; }
        public string? Content { get; set; }
        public string? TemplateName { get; set; }
        public string? Tenant { get; set; }
        public string? Email { get; set; }
        public int? ExecutionId { get; set; }
    }
}
