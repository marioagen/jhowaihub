namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Card status counts for a workflow audit: TotalCards, Finalized, Rejected.
    /// </summary>
    public record WorkflowAuditCardStatusCountResponseDto
    {
        public int TotalCards { get; init; }
        public int Finalized { get; init; }
        public int Rejected { get; init; }
    }
}
