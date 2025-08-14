namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeamId { get; set; }
        public DateTime Created { get; set; }
        public TeamDto Team { get; set; }
        public ICollection<StepDto> Steps { get; set; } = [];
        public int NumDocuments { get; set; } = 0;
    }
}
