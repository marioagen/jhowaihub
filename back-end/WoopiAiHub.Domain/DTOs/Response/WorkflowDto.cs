namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public ICollection<TeamDto> Teams { get; set; } = [];
        public ICollection<StepDto> Steps { get; set; } = [];
        public int NumDocuments { get; set; } = 0;
        public bool HasPendingToolUpdate { get; set; } = false;
    }
}
