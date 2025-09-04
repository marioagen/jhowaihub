using System.Text.Json.Nodes;

namespace WoopiAiHub.Domain.DTOs
{
    public class AddDocumentsRequestRefitDto
    {
        public string text { get; set; }
        public dynamic metadata { get; set; }
        public string? Tenant { get; set; }
        public string embeddings_model_name { get; set; } = string.Empty;
        public int Chunk_size { get; set; }
    }
}
