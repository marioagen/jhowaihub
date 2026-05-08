namespace WoopiAiHub.Domain.DTOs
{
    public record class AutomationServicesDto(int StepToolId, int CardId, string Tenant, string Email, string? ReferenceFile, int? StepId, int? WorkflowId = null);
}
