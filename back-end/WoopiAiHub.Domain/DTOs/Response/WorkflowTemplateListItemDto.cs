namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowTemplateListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public int StepCount { get; set; }
        public List<string> TeamNames { get; set; } = [];
        public List<string> RequiredSecrets { get; set; } = [];
    }
}
