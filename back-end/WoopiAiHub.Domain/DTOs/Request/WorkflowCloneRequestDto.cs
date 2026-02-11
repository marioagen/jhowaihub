namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// DTO for cloning an existing workflow.
    /// </summary>
    public record class WorkflowCloneRequestDto
    {
        /// <summary>
        /// ID of the workflow to clone.
        /// </summary>
        public int SourceWorkflowId { get; set; }

        /// <summary>
        /// Name for the new cloned workflow.
        /// </summary>
        public string NewName { get; set; } = string.Empty;
    }
}
