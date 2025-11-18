namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 2 of workflow creation: Steps Management
    /// </summary>
    public record class WorkflowPhase2Dto
    {
        public int WorkflowId { get; set; }
        public ICollection<StepPhase2Dto> Steps { get; set; } = [];
    }

    /// <summary>
    /// Step DTO for Phase 2 - includes only step information without tools
    /// </summary>
    public record class StepPhase2Dto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public int ProfileId { get; set; }
        public int StatusId { get; set; }
    }
}
