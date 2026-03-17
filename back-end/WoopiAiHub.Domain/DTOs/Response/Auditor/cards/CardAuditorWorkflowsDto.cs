namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Workflow id, name, step (id and name), and card id for auditor document view.
    /// </summary>
    public record CardAuditorWorkflowsDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int CardId { get; init; }
    }
}
