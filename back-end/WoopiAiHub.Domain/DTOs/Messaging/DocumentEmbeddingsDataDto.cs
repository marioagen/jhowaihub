namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public class DocumentEmbeddingsDataDto
    {
        public string ResponseQueue { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public ICollection<DocumentEmbeddingsAddDto> DocumentEmbeddings { get; set; } = [];
    }
}
