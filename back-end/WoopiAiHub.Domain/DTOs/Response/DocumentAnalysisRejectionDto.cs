namespace WoopiAiHub.Domain.DTOs.Response
{
    public record DocumentAnalysisRejectionDto
    {
        public int Id { get; init; }
        public string Justification { get; init; } = string.Empty;
        public int CardId { get; init; }
        public int StepId { get; init; }
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public DateTime Date { get; init; }
    }
}
