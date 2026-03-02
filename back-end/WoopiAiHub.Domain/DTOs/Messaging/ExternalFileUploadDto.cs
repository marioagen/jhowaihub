namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record class ExternalFileUploadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileReference { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
    }
}
