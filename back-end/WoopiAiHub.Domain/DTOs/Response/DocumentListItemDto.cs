using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class DocumentListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<DocumentWorkflowProgressDto> WorkflowProgress { get; set; } = new List<DocumentWorkflowProgressDto>();
    }
}
