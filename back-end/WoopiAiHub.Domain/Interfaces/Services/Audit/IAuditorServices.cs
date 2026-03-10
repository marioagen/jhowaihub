using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.Interfaces.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Provides access to documents, workflows, and users for auditing.
    /// </summary>
    public interface IAuditorServices
    {
        Task<ICollection<AuditorDocumentDto>> GetDocumentsAsync(int take, string? search, int? statusId);
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<ICollection<WorkflowDto>> GetWorkflowsAsync();
        Task<WorkflowDto?> GetWorkflowByIdAsync(int id);
        Task<ICollection<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
    }
}
