namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Documents
{
    public record DocumentAuditorSummaryRowDto
    {
        public int DocumentId { get; init; }
        public string DocumentName { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int CardId { get; init; }
        public string CardStatusName { get; init; } = string.Empty;

        /// <summary>False when the document is soft-deleted (<c>Enable == false</c>).</summary>
        public bool DocumentEnabled { get; init; } = true;
    }
}
