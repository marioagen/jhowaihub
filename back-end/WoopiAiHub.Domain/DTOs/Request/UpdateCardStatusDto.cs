namespace WoopiAiHub.Domain.DTOs.Request
{
    public record struct UpdateCardStatusDto
    {
        public int CardId { get; set; }
        public int StatusId { get; set; }
    }
}
