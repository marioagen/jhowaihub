using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    /// <summary>
    /// Repository for auditor-related queries. Used to list and retrieve documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorRepository
    {
        Task<ICollection<DocumentDto>> GetDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<ICollection<WorkflowDto>> GetWorkflowsAsync();
        Task<WorkflowDto?> GetWorkflowByIdAsync(int id);
        Task<ICollection<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
    }
}
