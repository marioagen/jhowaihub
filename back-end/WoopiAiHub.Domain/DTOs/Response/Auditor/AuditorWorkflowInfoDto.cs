namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Workflow id and name for auditor document view.
    /// </summary>
    public record AuditorWorkflowInfoDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
