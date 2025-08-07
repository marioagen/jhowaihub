namespace WoopiAiHub.Domain.DTOs.Response
{
    public class WorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public ICollection<StepDto> Steps { get; set; } = [];
    }
}
