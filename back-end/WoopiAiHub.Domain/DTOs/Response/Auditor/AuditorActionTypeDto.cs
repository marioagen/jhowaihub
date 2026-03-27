namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record AuditorActionTypeDto
    {
        public int Code { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
