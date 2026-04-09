namespace WoopiAiHub.Domain.DTOs.Response
{
    public class FindDocumentDto
    {
        public byte[]? BytesDocument { get; set; }
        public string ReferenceFile { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
    }
}
