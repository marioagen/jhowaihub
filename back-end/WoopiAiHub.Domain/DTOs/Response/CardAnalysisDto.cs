namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class CardAnalysisDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int StepId { get; set; }
        public int DocumentId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public StepDto? Step { get; set; }
        public DocumentDto? Document { get; set; }
        public ICollection<StepToolOutputAnalysesDto>? Outputs { get; set; }
    }
}
