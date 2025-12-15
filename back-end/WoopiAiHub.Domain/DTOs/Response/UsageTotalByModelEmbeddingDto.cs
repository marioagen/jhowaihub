namespace WoopiAiHub.Domain.DTOs.Response
{
    public record UsageTotalByModelEmbeddingDto
    {
        public int? ModelEmbeddingId { get; set; }
        public decimal Total { get; set; }
    }
}
