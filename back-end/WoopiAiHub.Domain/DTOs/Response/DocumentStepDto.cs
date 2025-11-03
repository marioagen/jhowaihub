namespace WoopiAiHub.Domain.DTOs.Response
{
    public class DocumentStepDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<ExtractedFieldDto> Outputs { get; set; } = new();
    }
}
