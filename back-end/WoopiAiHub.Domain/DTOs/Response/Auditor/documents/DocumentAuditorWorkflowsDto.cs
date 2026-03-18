namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Documents
{
    public record DocumentAuditorWorkflowsDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int DocumentId { get; init; }
    }
}
