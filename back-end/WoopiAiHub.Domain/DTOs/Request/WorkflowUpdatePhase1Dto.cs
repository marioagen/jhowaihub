namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 1 update: workflow id, name, optional description (max 500 characters) and teams.
    /// </summary>
    public record class WorkflowUpdatePhase1Dto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> Teams { get; set; } = [];
        public string Description { get; set; } = string.Empty;
    }
}
