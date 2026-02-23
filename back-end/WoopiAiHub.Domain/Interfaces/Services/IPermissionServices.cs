using WoopiAiHub.Domain.DTOs;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IPermissionServices
    {
        List<GroupedPermissionsDto> FindAll();
        ICollection<PermissionDto> FindWorkflowPermissions();
        Task<bool> UserHasPermissionAsync(string email, string group, string permission);
    }
}
