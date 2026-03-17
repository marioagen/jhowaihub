namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Rows
{
    /// <summary>
    /// Raw audit row for document detail. Used by repository and service to build DocumentAuditorDetailDto.
    /// </summary>
    public record DocumentAuditorDetailRowDto
    {
        public string DocumentName { get; init; } = string.Empty;
        public string WorkflowName { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string ActionName { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public DateTime Created { get; init; }
    }
}
