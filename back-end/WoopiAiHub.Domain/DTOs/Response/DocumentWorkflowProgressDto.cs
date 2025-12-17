namespace WoopiAiHub.Domain.DTOs.Response
{
    public class DocumentWorkflowProgressDto
    {
        public string WorkflowName { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
    }
}
