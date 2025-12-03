namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ModelEmbeddingDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
