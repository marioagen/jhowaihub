namespace WoopiAiHub.Domain.DTOs
{
    public record struct StepToolDependencyDto
    {
        public int StepToolOrder { get; set; }
        public int StepOrder { get; set; }
    }
}
