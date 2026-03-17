namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Document status counts for a workflow audit: TotalDocuments, Finalized, Rejected (at document level).
    /// </summary>
    public record WorkflowAuditorDocumentStatusCountDto
    {
        public int TotalDocuments { get; init; }
        public int Finalized { get; init; }
        public int Rejected { get; init; }
    }
}
