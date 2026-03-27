namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 1 of workflow creation: Name, optional description (max 500 characters) and team associations.
    /// </summary>
    public record class WorkflowPhase1Dto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> Teams { get; set; } = [];
        public string Description { get; set; } = string.Empty;
    }
}
