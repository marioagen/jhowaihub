namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Documents
{
    /// <summary>
    /// One audit history entry for document detail: UserId, UserName, ActionName, StepId, StepName, Created.
    /// </summary>
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
