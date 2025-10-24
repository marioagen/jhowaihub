namespace WoopiAiHub.Domain.DTOs.Connector
{
    public record class FormFieldDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int? MaxLength {  get; set; }
        public int? MinLength { get; set; }
        public bool Required { get; set; }
        public List<FormFieldDto>? Children { get; set; }
    }
}
