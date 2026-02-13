namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ApiOutputDto
    {
        public string TemplateName { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ExecutionId { get; set; }
        public int StatusCode { get; set; }
        public string? Content { get; set; }
    }
}
