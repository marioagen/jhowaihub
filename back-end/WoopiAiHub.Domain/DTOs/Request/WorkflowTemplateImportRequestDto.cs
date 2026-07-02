namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WorkflowTemplateImportRequestDto
    {
        public List<Guid> TemplateIds { get; set; } = [];
        public Dictionary<string, string> SecretValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
