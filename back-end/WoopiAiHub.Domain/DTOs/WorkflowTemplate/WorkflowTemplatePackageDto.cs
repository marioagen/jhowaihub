namespace WoopiAiHub.Domain.DTOs.WorkflowTemplate
{
    public record class WorkflowTemplatePackageDto
    {
        public string SchemaVersion { get; set; } = "1.1";
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public DateTime Created { get; set; }
        public List<string> TeamNames { get; set; } = [];
        public List<string> TeamCodes { get; set; } = [];
        public List<string> RequiredSecrets { get; set; } = [];
        public List<WorkflowTemplateStepDto> Steps { get; set; } = [];
        public List<WorkflowTemplatePromptDto> Prompts { get; set; } = [];
        public List<WorkflowTemplateApiTemplateDto> ApiTemplates { get; set; } = [];
    }

    public record class WorkflowTemplateStepDto
    {
        public int Order { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public List<WorkflowTemplateStepToolDto> StepTools { get; set; } = [];
    }

    public record class WorkflowTemplateStepToolDto
    {
        public int Order { get; set; }
        public string ToolType { get; set; } = string.Empty;
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public List<WorkflowTemplateDependencyDto> Dependencies { get; set; } = [];
        public List<WorkflowTemplateParameterDto> Parameters { get; set; } = [];
    }

    public record class WorkflowTemplateDependencyDto
    {
        public int StepOrder { get; set; }
        public int StepToolOrder { get; set; }
    }

    public record class WorkflowTemplateParameterDto
    {
        public string Value { get; set; } = string.Empty;
        public bool RequiredFile { get; set; }
        public Guid? WebhookId { get; set; }
    }

    public record class WorkflowTemplatePromptDto
    {
        public string Ref { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool EnableAccessToMcp { get; set; }
    }

    public record class WorkflowTemplateApiTemplateDto
    {
        public string Ref { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? QueryTemplate { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? Description { get; set; }
        public bool EnableAccessFromMcp { get; set; }
    }
}
