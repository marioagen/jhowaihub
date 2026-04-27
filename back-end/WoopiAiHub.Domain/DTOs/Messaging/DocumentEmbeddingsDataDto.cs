using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class DocumentEmbeddingsDataDto
    {
        public RagProvider? RagProvider { get; set; }
        public string ApplicationId { get; set; } = string.Empty;
        public string ApplicationKey { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string ResponseQueue { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public MetaDataAutomationDto Data { get; set; }
        public ICollection<DocumentEmbeddingsAddDto> DocumentEmbeddings { get; set; } = [];
    }
}
