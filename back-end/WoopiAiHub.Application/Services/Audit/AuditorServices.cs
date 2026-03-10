using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Services.Audit;

namespace WoopiAiHub.Application.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Delegates to auditor repository.
    /// </summary>
    public class AuditorServices : IAuditorServices
    {
        private readonly IAuditorRepository _auditorRepository;

        public AuditorServices(IAuditorRepository auditorRepository)
        {
            _auditorRepository = auditorRepository;
        }

        public Task<ICollection<AuditorDocumentDto>> GetDocumentsAsync(int take, string? search, int? statusId)
            => _auditorRepository.GetDocumentsAsync(take, search, statusId);

        public Task<DocumentDto?> GetDocumentByIdAsync(int id)
            => _auditorRepository.GetDocumentByIdAsync(id);

        public Task<ICollection<WorkflowDto>> GetWorkflowsAsync()
            => _auditorRepository.GetWorkflowsAsync();

        public Task<WorkflowDto?> GetWorkflowByIdAsync(int id)
            => _auditorRepository.GetWorkflowByIdAsync(id);

        public Task<ICollection<UserDto>> GetUsersAsync()
            => _auditorRepository.GetUsersAsync();

        public Task<UserDto?> GetUserByIdAsync(Guid id)
            => _auditorRepository.GetUserByIdAsync(id);
    }
}
