using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepProfilePermissionsRepository
    {
        Task<bool> Create(int ProfileId, List<WorkflowPermissionDto> PermissionsWorkflow);
        Task<bool> DeleteAsync(int ProfileId);
    }
}
