namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record UsersAuditorActionTypeCountsDto
    {
        public int ActionTypeCode { get; init; }
        public int Count { get; init; }
    }
}
