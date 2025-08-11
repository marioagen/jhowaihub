namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WorkflowCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public ICollection<StepCreateDto> Steps { get; set; } = [];
    }
}
