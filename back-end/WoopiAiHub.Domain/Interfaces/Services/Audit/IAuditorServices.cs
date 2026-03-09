using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Provides access to documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorServices
    {
        Task<ICollection<DocumentDto>> GetDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<ICollection<WorkflowDto>> GetWorkflowsAsync();
        Task<WorkflowDto?> GetWorkflowByIdAsync(int id);
        Task<ICollection<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
    }
}
