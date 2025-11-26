namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowDto
    {
        public int Id { get; set; } //etapa 1 apenas trazer id nome e times etapa 2 steps 3
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public ICollection<TeamDto> Teams { get; set; } = [];
        public ICollection<StepDto> Steps { get; set; } = [];
        public int NumDocuments { get; set; } = 0;
    }
}
