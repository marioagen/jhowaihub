namespace WoopiAiHub.Domain.DTOs.Request
{
    public record AssignRangeDto(Guid UserId, List<int> CardIds);
}
