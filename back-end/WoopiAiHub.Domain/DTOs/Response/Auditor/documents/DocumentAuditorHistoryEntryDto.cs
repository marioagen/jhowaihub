namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Documents
{
    public record DocumentAuditorHistoryEntryDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string ActionName { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public DateTime Created { get; init; }
    }
}
