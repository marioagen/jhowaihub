namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class DocumentHistoryDto
    {
        public int Id { get; set; }
        public int IdDocument { get; set; }
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public bool IsEdited { get; set; }
        public int? Type { get; set; }
        public Guid? UserId { get; set; }
        public DateTime Created { get; set; }
    }
}
