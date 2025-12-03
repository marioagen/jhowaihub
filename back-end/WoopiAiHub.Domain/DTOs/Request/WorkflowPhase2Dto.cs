namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 2 of workflow creation: Steps Management
    /// </summary>
    public record class WorkflowPhase2Dto
    {
        public int WorkflowId { get; set; }
        public ICollection<StepPhase2Dto> Steps { get; set; } = [];
        public bool HasStepTool { get; set; } = false;
    }
}
