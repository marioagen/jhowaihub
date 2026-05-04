namespace WoopiAiHub.Domain.DTOs.Messaging
{
    public record struct MetaDataAutomationDto(int CardId, int StepToolId, int? WorkflowId = null);
}
