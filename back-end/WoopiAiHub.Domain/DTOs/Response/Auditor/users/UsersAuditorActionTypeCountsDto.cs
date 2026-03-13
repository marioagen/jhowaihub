namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Action type code (AuditCardActionType enum value) and count for user audit details. Front-end can translate code to human-readable label.
    /// </summary>
    public record UsersAuditorActionTypeCountsDto
    {
        public int ActionTypeCode { get; init; }
        public int Count { get; init; }
    }
}
