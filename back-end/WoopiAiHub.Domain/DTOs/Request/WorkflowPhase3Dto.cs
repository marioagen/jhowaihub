namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 3 of workflow creation: Tool Flows Configuration
    /// </summary>
    public record class WorkflowPhase3Dto
    {
        public int WorkflowId { get; set; }
        public ICollection<StepPhase3Dto> Steps { get; set; } = [];
    }

    /// <summary>
    /// Step DTO for Phase 3 - includes step tools/ferramentas
    /// </summary>
    public record class StepPhase3Dto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public ICollection<StepToolUpdateDto> StepTools { get; set; } = [];
    }
}
