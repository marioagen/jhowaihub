namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for Phase 3 of workflow creation: Tool Flows Configuration
    /// </summary>
    public record class WorkflowPhase3Dto
    {
        public int WorkflowId { get; set; }
        public ICollection<StepPhase3Dto> Steps { get; set; } = [];
        public bool ResetDocuments { get; set; } = false;
    }
}
