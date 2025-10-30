namespace WoopiAiHub.Domain.DTOs
{
    public class StepToolDependencyDto
    {
        public int Id { get; set; }
        public int StepToolId { get; set; }
        public int DependsOnStepToolId { get; set; }
        public string DependsOnStepToolName { get; set; } = string.Empty;
        public int DependsOnStepOrder { get; set; }
    }
}
