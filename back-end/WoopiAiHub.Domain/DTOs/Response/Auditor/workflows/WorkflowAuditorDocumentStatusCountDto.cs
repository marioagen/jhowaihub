namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record WorkflowAuditorDocumentStatusCountDto
    {
        public int TotalDocuments { get; init; }
        public int Finalized { get; init; }
        public int Rejected { get; init; }
    }
}
