namespace WoopiAiHub.Domain.Utils
{
    public class WorkflowTemplateSettings
    {
        public string TemplateFileName { get; set; } = "workflow-templates.json";
        public string Folder { get; set; } = "WorkflowTemplate";
        public string LocalCatalogPath { get; set; } = "Data/workflow-templates.json";
    }
}
