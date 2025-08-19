namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WorkflowUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public ICollection<StepUpdateDto> Steps { get; set; } = [];
    }
}
