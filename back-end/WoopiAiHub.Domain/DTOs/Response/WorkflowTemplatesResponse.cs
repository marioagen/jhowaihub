using WoopiAiHub.Domain.DTOs.WorkflowTemplate;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class WorkflowTemplatesResponse
    {
        public List<WorkflowTemplatePackageDto> Workflows { get; set; } = [];
    }
}
