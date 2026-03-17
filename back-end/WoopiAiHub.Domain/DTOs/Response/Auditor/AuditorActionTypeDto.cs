namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Action type option for auditor filters: code (AuditCardActionType enum value) and display name.
    /// </summary>
    public record AuditorActionTypeDto
    {
        public int Code { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
