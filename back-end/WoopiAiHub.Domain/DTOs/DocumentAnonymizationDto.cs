namespace WoopiAiHub.Domain.DTOs
{
    public record DocumentAnonymizationDto
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
