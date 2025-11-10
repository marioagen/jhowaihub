namespace WoopiAiHub.Domain.DTOs.Request
{
    public record struct StepToolOutputDependencyDto
    {
        public int StepOrder { get; set; }
        public int StepToolOrder { get; set; }
    }
}
