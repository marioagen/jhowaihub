namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class DocumentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
    }
}
