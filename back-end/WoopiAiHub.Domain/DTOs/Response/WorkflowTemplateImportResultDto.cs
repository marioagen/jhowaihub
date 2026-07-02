namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowTemplateImportResultDto
    {
        public Guid TemplateId { get; set; }
        public int WorkflowId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
