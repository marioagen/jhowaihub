namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 1 of workflow creation: Name and Team Associations
    /// </summary>
    public record class WorkflowPhase1Dto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> Teams { get; set; } = [];
    }
}
