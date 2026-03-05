namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class CardBatchDto
    {
        public int CardId { get; set; }
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
    }
}
